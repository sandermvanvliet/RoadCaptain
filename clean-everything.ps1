# Copyright (c) 2025 Sander van Vliet
# Licensed under Artistic License 2.0
# See LICENSE or https://choosealicense.com/licenses/artistic-2.0/
# This script is to help with ensuring a clean environment when switching
# runtime identifiers or target frameworks.
# Usually you'll run into problems because the package caches get left around
# and nothing will compile because dotnet build will complain.
get-childitem -Include bin,obj -Recurse |  Remove-Item -Recurse -Path { $_.FullName }
