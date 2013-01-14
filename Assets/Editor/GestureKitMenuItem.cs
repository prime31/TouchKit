using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;



public class GestureKitMenuItem : Editor
{
	const string
		kSourcePath = "Assets/Plugins/GestureKit/",
		kBuildTarget = "Assets/Plugins/GestureKit/GestureKit.dll";
	
	
	[MenuItem( "TouchKit/Create DLL..." )]
	static void createDLL()
	{
		var compileParams = new CompilerParameters();
		compileParams.OutputAssembly = kBuildTarget;
		compileParams.CompilerOptions = "/optimize";
		compileParams.ReferencedAssemblies.Add( Path.Combine( EditorApplication.applicationContentsPath, "Frameworks/Managed/UnityEngine.dll" ) );
		
		var source = getSourceForStandardDLL( kSourcePath );

		var codeProvider = new CSharpCodeProvider( new Dictionary<string, string> { { "CompilerVersion", "v3.0" } } );
    	var compilerResults = codeProvider.CompileAssemblyFromSource( compileParams, source );
		
    	if( compilerResults.Errors.Count > 0 )
    	{
    		foreach( var error in compilerResults.Errors )
    		{
    			Debug.LogError( error.ToString() );
    		}
		}
		
    	AssetDatabase.Refresh();
	}
	
	
	static string[] getSourceForStandardDLL( string path )
	{
		var source = new List<string>();

		foreach( var file in Directory.GetFiles( path, "*.cs" ) )
		{
			if( !file.Contains( "GestureKitEditorSupport" ) )
				source.Add( File.ReadAllText( file ) );
		}
		
		foreach( var dir in Directory.GetDirectories( path ) )
			source.AddRange( getSourceForStandardDLL( dir ) );
		
		return source.ToArray();
	}

}
