.PHONY: all

all:
	ls *.cs | entr -r sh -c "mcs -d:TRACE /reference:System.Drawing.dll main.cs && ./main.exe" | mark

lena-256x128.png:
	convert refimg/lena-512x512.tif -resize 256x128! refimg/lena-256x128.png

lena-512x512.png:
	convert refimg/lena-512x512.tif -resize 512x512! refimg/lena-512x512.png
