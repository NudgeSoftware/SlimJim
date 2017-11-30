Microsoft Visual Studio Solution File, Format Version $sln.Version.SlnFileVersionNumber$
# Visual Studio $sln.Version.SlnVisualStudioVersion$
VisualStudioVersion = 15.0.27004.2010
MinimumVisualStudioVersion = 10.0.40219.1
$sln.Projects:{ p | Project("$p.ProjectTypeGuid$") = "$p.AssemblyName$", "$p.Path$", "$p.Guid$"
EndProject};separator="\r\n"$
$if(sln.Folders)$
$sln.Folders:{ f | Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "$f.FolderName$", "$f.FolderName$", "$f.Guid$"
EndProject};separator="\r\n"$
$endif$


Global
$if(sln.Projects)$
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|x64 = Debug|x64
		Release|x64 = Release|x64
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
	$sln.Projects:{ p |
	$p.Guid$.Debug|x64.ActiveCfg = Debug|x64
	$p.Guid$.Debug|x64.Build.0 = Debug|x64
	$p.Guid$.Release|x64.ActiveCfg = Release|x64
	$p.Guid$.Release|x64.Build.0 = Release|x64};separator="\r\n"$
	EndGlobalSection

$endif$
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
$if(sln.Folders)$
	GlobalSection(NestedProjects) = preSolution
		$sln.Folders:{ f | $f.ContentGuids:{ g | $g$ = $f.Guid$};separator="\r\n"$};separator="\r\n"$
	EndGlobalSection

$endif$
EndGlobal
