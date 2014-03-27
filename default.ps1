properties { 
  $base_dir  = resolve-path .
  $build_dir = "$base_dir\build"
  $packageinfo_dir = "$base_dir"
  $debug_build_dir = "$build_dir\bin\debug"
  $release_build_dir = "$build_dir\bin\release"
  $release_dir = "$base_dir\release"
  $sln_file = "$base_dir\src\SlimJim.sln"
  $version = "1.1.0"
  $revision = ""
  $tools_dir = "$base_dir\Tools"
  $nunitconsole = "nunit-console.exe"
  $run_tests = $true
}

Framework "4.0"

include .\psake_ext.ps1

task default -depends Package

task Clean {
  if (Test-Path $build_dir) { remove-item -force -recurse $build_dir }
  if (Test-Path $release_dir) { remove-item -force -recurse $release_dir }
}

task Init -depends Clean {
	mkdir @($release_dir, $build_dir) | out-null
	
	Generate-Assembly-Info `
		-file "$build_dir\VersionInfo.cs" `
		-revision $revision `
		-version $version
}

task Compile -depends Init {
  Exec { msbuild $sln_file /p:"Configuration=Debug;TargetFrameworkVersion=v4.0" } "msbuild (debug) failed."
  Exec { msbuild $sln_file /p:"OutDir=$release_build_dir\;Configuration=Release;TargetFrameworkVersion=v4.0" } "msbuild (release) failed."
}

task Test -depends Compile -precondition { return $run_tests }{
  Exec { & $nunitconsole "src\SlimJim.Test\bin\Debug\SlimJim.Test.dll" "/noshadow" "/result=build\SlimJim.Test.xml" } "nunit failed."
}

task Package -depends Compile, Test {

  $spec_files = @(Get-ChildItem $packageinfo_dir "*.nuspec" -Recurse)

  foreach ($spec in @($spec_files))
  {
	$dir =  $($spec.Directory)
	cd $dir
    Exec { nuget pack -o $release_dir -Properties Configuration=Release`;OutDir=$release_build_dir\ -Version $version -Symbols } "nuget pack failed."
  }
}
