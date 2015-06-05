properties {
    $buildDir = ".\build"
    $outputDir = "$buildDir\lib\$framework"
    $shortFramework = $framework.Replace('.','')
    $libs = "packages\Ninject.3.0.1.10\lib\net$shortFramework,packages\Moq.4.0.10827\lib\NET$shortFramework"
}

task default -depends Compile

task Compile -depends Clean,Init { 
    $sources = gci ".\MoqBot" -r -fi *.cs |% { $_.FullName }
    $out = $outputDir + "\MoqBot.dll"
    csc /target:library /out:$out $sources /lib:$libs /r:Ninject.dll /r:Moq.dll
}

task Init {
    mkdir $outputDir | out-null
}

task Clean { 
    if (test-path $outputDir) { ri -r -fo $outputDir }
    echo $libs
}
