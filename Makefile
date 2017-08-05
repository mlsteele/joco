.PHONY: all

all:
	ls *.cs | entr -r sh -c "mcs -d:TRACE /reference:System.Drawing.dll main.cs && ./main.exe" | mark

lena-256x128.png:
	convert lena-512x512.tif -resize 256x128! lena-256x128.png

lena-512x512.png:
	convert lena-512x512.tif -resize 512x512! lena-512x512.png
