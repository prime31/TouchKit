#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;



public class TouchKitMenuItem : Editor
{
	const string
		kSourcePath = "Assets/TouchKit/",
		kBuildTargetFilename = "TouchKit.dll";


	[MenuItem( "TouchKit/Create TouchKit.dll..." )]
	static void createDLL()
	{
		var compileParams = new CompilerParameters();
		compileParams.OutputAssembly = Path.Combine( System.Environment.GetFolderPath( System.Environment.SpecialFolder.Desktop ), kBuildTargetFilename );
		compileParams.CompilerOptions = "/optimize";
		compileParams.ReferencedAssemblies.Add( Path.Combine( EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll" ) );

		var source = getSourceForStandardDLL( kSourcePath );

		var codeProvider = new CSharpCodeProvider( new Dictionary<string, string> { { "CompilerVersion", "v3.0" } } );
    	var compilerResults = codeProvider.CompileAssemblyFromSource( compileParams, source );

    	if( compilerResults.Errors.Count > 0 )
    	{
    		foreach( var error in compilerResults.Errors )
    			Debug.LogError( error.ToString() );
		}
		else
		{
			EditorUtility.DisplayDialog( "TouchKit", "TouchKit.dll should now be on your desktop. If you would like the in-editor support (multi-touch simulator and debug drawing of touch frames) copy the TouchKit/Editor/TouchKitEditor.cs file into your project along with the TouchKit.dll", "OK" );
		}
	}


	static string[] getSourceForStandardDLL( string path )
	{
		var source = new List<string>();

		foreach( var file in Directory.GetFiles( path, "*.cs" ) )
		{
			if( !file.Contains( "TouchKitEditor" ) && !file.Contains( "Demo" ) && !file.Contains( "Virtual" ) )
				source.Add( File.ReadAllText( file ) );
		}

		foreach( var dir in Directory.GetDirectories( path ) )
		{
			if( !dir.Contains( "/Editor" ) )
				source.AddRange( getSourceForStandardDLL( dir ) );
		}

		return source.ToArray();
	}

}
#endif
