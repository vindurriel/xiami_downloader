@echo off
set /p comment="comments:"
git commit -am "%comment%"
git push
python ./python_xiami_api/submitter.py