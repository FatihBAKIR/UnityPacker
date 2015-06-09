# UnityPacker

UnityPacker is a small command line tool that can create `UnityPackage` files without a Unity installation. It is great for automated builds of Unity tools.

Usage is very simple:

    ./UnityPacker *directory to pack* *destination pack name*
    
For example:

    ./UnityPacker . Package
    
Will produce a `Package.unitypackage` from the contents current directory recursively in the current directory.

+ When used with `meaningfulHashes` option, packer will even maintain prefab components 
and even complete scenes if they are included in the directory.

+ It will automatically skip `.meta` files, however you can add more using the `extensions` option. 
You can skip whole directories with the `directories` option.

+ Sizes of the packages exported from Unity itself and UnityPacker are usually the same, 
however for big packages, UnityPacker will produce smaller packages due to not including unnecessary meta files directly.

### Use Case

A good use case is to pack your assets automatically in your server.

For example, I use it to build the [Quark.Default](https://github.com/FatihBAKIR/Quark.Default) 
package whenever I push a new commit using the following script:

	cd /tmp ;
	git clone https://github.com/FatihBAKIR/Quark.Default ;
	cd Quark.Default ;
	wget https://github.com/FatihBAKIR/UnityPacker/releases/download/0.0.1/UnityPacker.exe ;
	wget https://github.com/FatihBAKIR/UnityPacker/releases/download/0.0.1/ICSharpCode.SharpZipLib.dll ;
	chmod +x UnityPacker.exe ;
	./UnityPacker.exe . QuarkDefault no "Assets/QuarkDefault/" "gitignore,md,exe,dll" ".git" ;
	mv QuarkDefault.unitypackage ~ ;
	rm -rf /tmp/Quark.Default ;

Let's focus on the line I'm using UnityPacker:

1. `.` : For the current directory (i.e. `/tmp/Quark.Default`).
2. `QuarkDefault` : Name of the output package.
3. `no` : There are no meta files, create random hashes.
4. `"Assets/QuarkDefault"` : This argument determines the root path of the extracted contents when unpacked with Unity.
5. `"gitignore,md,exe,dll"` : Skipped file extensions. Files with these extensions will not be included in the output package.
6. `".git"` : Similar to the 5th argument. Files in the .git directory will not be included in the output package.

*Was tested on Unity 4 and 5 with packages from both Windows and Linux hosts.*
