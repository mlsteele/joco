.PHONY: all

all:
	ls *.cs | entr -r sh -c "mcs -d:TRACE /reference:System.Drawing.dll main.cs && ./main.exe" | mark

lena-256x128.png:
	convert lena-1024x1024.png -resize 256x128! lena-256x128.png
