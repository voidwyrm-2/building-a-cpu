#! /bin/sh

# if a second argument is given and is 'y', then the resulting build is moved to the gobin(if is exists)
buildProj() {
    # shellcheck disable=SC2164
    cd "./$1"

    # FREAKING BUILD BACKUPS MAKING MY CHANGES NOT EFFECT THE BUILD YOU FREAKING PIECE OF CRAP I HATE YOU
    rm -r "./obj/Debug"

    dotnet build

    if [ $# -gt 1 ]; then
        if [ "$2" = "y" ]; then
            if ! [ -f "$HOME/go/bin" ]; then
                mv "./bin/Debug/net8.0/$1" "$HOME/go/bin/$1"

                mv "./bin/Debug/net8.0/$1.deps.json" "$HOME/go/bin/$1.deps.json"

                mv "./bin/Debug/net8.0/$1.dll" "$HOME/go/bin/$1.dll"

                mv "./bin/Debug/net8.0/$1.pdb" "$HOME/go/bin/$1.pdb"

                mv "./bin/Debug/net8.0/$1.runtimeconfig.json" "$HOME/go/bin/$1.runtimeconfig.json"

                echo "moved binaries to gobin"
            fi
        fi
    fi

    # shellcheck disable=SC2103
    cd ..
}

buildProj "basm" "y"
buildProj "smc4" "y"