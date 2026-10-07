#!/bin/bash

# Builds and runs the Melissa Reverse GeoCoder Cloud API .NET sample.
#
# This script builds ReverseGeoCoderDotnet with dotnet publish, then runs the resulting
# executable, passing along the license and (if supplied) the coordinates and max records.
#
# Overall flow:
#   1. Parse the command-line options below.
#   2. Resolve the license (--license, then a prompt, then the MD_LICENSE environment variable).
#   3. Publish ReverseGeoCoderDotnet in Release configuration to ./ReverseGeoCoderDotnet/Build.
#   4. Run the built executable: one-shot mode if --lat, --long, or --max was supplied,
#      otherwise interactive mode (the .NET program prompts for each field).
#
# Options (each takes a value):
#   --lat       Latitude to look up.
#   --long      Longitude to look up (negative values are allowed).
#   --max       Maximum number of records to return.
#   --license   License string. If omitted, the script prompts for it; if the prompt
#               is left blank, it falls back to MD_LICENSE. Running without --license
#               always prompts, even when MD_LICENSE is set.
#
# Paths are relative to the current directory, so run the script from its own folder.
#
# Examples:
#   ./ReverseGeoCoderDotnet.sh --license "your-license"
#   ./ReverseGeoCoderDotnet.sh --lat "33.637520" --long "-117.606920" --max "3" --license "your-license"

######################### Constants ##########################

RED='\033[0;31m' #RED
NC='\033[0m' # No Color

######################### Parameters ##########################

lat=""
long=""
max=""
license=""

# Read each --flag and its value. A flag with no value, or whose value looks like an
# option name (such as --license), is an error; other values starting with "-", such as
# negative numbers, are allowed. Unrecognized options are ignored.
while [ $# -gt 0 ] ; do
  case $1 in
    --lat)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'lat'.${NC}\n"
            exit 1
        fi
        lat="$2"
        shift
        ;;
    --long)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'long'.${NC}\n"
            exit 1
        fi
        long="$2"
        shift
        ;;
    --max)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'max'.${NC}\n"
            exit 1
        fi
        max="$2"
        shift
        ;;
    --license)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'license'.${NC}\n"
            exit 1
        fi
        license="$2"
        shift
        ;;
  esac
  shift
done


# Build paths are relative to the current directory (not the script's location)
CurrentPath="$(pwd)"
ProjectPath="$CurrentPath/ReverseGeoCoderDotnet"
BuildPath="$ProjectPath/Build"

if [ ! -d "$BuildPath" ];
then
    mkdir "$BuildPath"
fi

########################## Main ############################
printf "\n=================== Melissa Reverse GeoCoder Cloud API =====================\n"

# Get license (either from parameters or user input)
if [ -z "$license" ];
then
  printf "Please enter your license string: "
  read license
fi

# Check for License from Environment Variables 
if [ -z "$license" ];
then
  license=`echo $MD_LICENSE` 
fi

if [ -z "$license" ];
then
  printf "\nLicense String is invalid!\n"
  exit 1
fi

# Start program
# Build project
printf "\n=============================== BUILD PROJECT ==============================\n"

dotnet publish -f="net7.0" -c Release -o "$BuildPath" ReverseGeoCoderDotnet/ReverseGeoCoderDotnet.csproj

# Run project
# None of lat, long, or max supplied -> run interactively; otherwise pass them through for
# one-shot mode.
# Bash passes empty quoted values as real empty arguments, so unsupplied fields arrive
# empty and the program prompts for them.
if [ -z "$lat" ] && [ -z "$long" ] && [ -z "$max" ];
then
    dotnet "$BuildPath"/ReverseGeoCoderDotnet.dll --license "$license"
else
    dotnet "$BuildPath"/ReverseGeoCoderDotnet.dll --license "$license" --lat "$lat" --long "$long" --max "$max"
fi

