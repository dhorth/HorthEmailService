bfg --replace-text secrets.txt
git reflog expire --expire=now --all && git gc --prune=now --aggressive