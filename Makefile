.PHONY: all

all:
	ls *.cs | entr -r sh -c "mcs -d:TRACE /reference:System.Drawing.dll main.cs && ./main.exe" | mark
