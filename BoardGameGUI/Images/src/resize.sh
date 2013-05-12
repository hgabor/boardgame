for file in `ls --color=none`; do convert -filter Lanczos -resize 30x30 $file out/$file ; done
