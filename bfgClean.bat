bfg --replace-text secrets.txt
git reflog expire --expire=now --all && git gc --prune=now --aggressive
git pull
git add .
git commit -m"CleanUp"
git push
