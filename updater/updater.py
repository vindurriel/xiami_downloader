#encoding=utf-8
"""
update.exe(or .py) check/update
"""
import os,sys,zipfile,requests
def cwd(*args):
	d=os.path.dirname(sys.argv[0])
	for x in args:
		d=os.path.join(d,x)
	return d
def check_latest():
	res=requests.get("https://api.github.com/repos/vindurriel/xiami_downloader/commits",verify=False)
	latest= res.json()[0]["sha"]
	file(cwd("latest.txt"),'w').write(latest)
	return latest
def fetch_latest():
	res=requests.get("https://github.com/vindurriel/xiami_downloader/archive/master.zip",verify=False)
	file(cwd('latest.zip'),'wb').write(res.content)
	z=zipfile.ZipFile(cwd('latest.zip'))
	dir_latest=cwd("latest")
	if os.path.isdir(dir_latest):
		import shutil
		shutil.rmtree(dir_latest)
	os.makedirs(dir_latest)
	filenames=[x for x in z.namelist() if x.startswith('xiami_downloader-master/Jean_Doe.Output')]
	for x in filenames:
		z.extract(x,dir_latest)
	file(cwd('needs_update'),'w').write('1')
def update():
	import shutil
	dir_src=cwd('latest','xiami_downloader-master','Jean_Doe.Output')
	if not os.path.isdir(dir_src):
		sys.exit(-1)
	files=os.listdir(dir_src)
	for f in files:
		shutil.copy2(os.path.join(dir_src,f),cwd())
	shutil.copy2(cwd('latest.txt'),cwd('current.txt'))
	if os.path.isfile(cwd('needs_update')):
		os.remove(cwd('needs_update'))
if __name__ == '__main__':
	try:
		if len(sys.argv)!=2:
			print __doc__
			raise Exception('needs 1 arg, got 0')
		cmd=sys.argv[1]
		if cmd=="check":
			latest=check_latest()
			current=""
			if os.path.isfile(cwd('current.txt')):
				current=file(cwd('current.txt'),'r').read()
			if latest!=current:
				fetch_latest()
		elif cmd=="update":
			update()
			import subprocess
			subprocess.Popen("xiami.exe")
		else:
			print __doc__
			raise Exception('invalid method')
	except Exception, e:
		import traceback
		traceback.print_exc()
		file(cwd('updater.log'),'a').write(traceback.format_exc())
		raw_input()
	