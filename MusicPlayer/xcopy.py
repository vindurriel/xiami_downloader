#!python
#encoding=utf-8
"""usage: xcopy.py <from_dir> <to_dir> <DATE_MODIFIED>
"""
def die(why=__doc__):
	print why
	exit(1)
def walk(rootdir):
	import os
	for root,dirs,files in os.walk(rootdir):
		for file in files:
			yield  os.path.join(root,file)
		for d in dirs:
			for x in walk(os.path.join(root,d)):
				yield  x
def find_newer(folder,timestamp):
	li=[]
	for x in walk(folder):
		import datetime,os
		m=datetime.datetime.fromtimestamp(os.path.getmtime(x))
		if m>=timestamp:
			li.append(x)
	return li
def parse_config(ini=".\\xcopy.ini"):
	config={}
	try:
		with open(ini,'r') as f:
			config=eval(f.read())
	except Exception, e:
			print e
	return config
def comply(file,rules):
	if type(rules)!=type({}) \
	or not "include_files" in rules \
	or not "exclude_files" in rules:
		return True
	from fnmatch import fnmatch
	ok=False
	for x in rules["include_files"]:
		if fnmatch(file,x):
			ok=True
			break
	if not ok: return False
	for x in rules["exclude_files"]:
		if fnmatch(file,x):
			return False
	return True
def main():
	config=parse_config()
	import sys
	if len(sys.argv)==1:
		from_dir,to_dir,date=config["from_dir"],config["to_dir"],config["date"]
	elif len(sys.argv)!=4:
		die("syntax error\n"+__doc__)
	else:
		from_dir,to_dir,date=sys.argv[1],sys.argv[2],sys.argv[3]
	import os
	from_dir=os.path.abspath(from_dir)
	to_dir=os.path.abspath(to_dir)
	if from_dir==to_dir:
		die("could not copy to the same dir")
	if not os.path.isdir(from_dir):
		die("dir not found: '%s'"%from_dir)
	import datetime
	try:
		mod=datetime.datetime.strptime(date,"%Y.%m.%d.%H.%M")
	except Exception, e:
		die("date parse error: "+str(e))
	if os.path.isdir(to_dir):
		import shutil
		shutil.rmtree(to_dir)
	li=find_newer(from_dir,mod)
	print len(li),"file(s) newer than",mod,"found"
	li=[x for x in li if comply(x,config)]
	for x in li:		
		destdir=os.path.dirname(x.replace(from_dir,to_dir))
		if not os.path.isdir(destdir):
			os.makedirs(destdir)
		import shutil
		shutil.copy2(x,destdir)
	print len(li),"file(s) comply with rules copied to",to_dir
	raw_input()
if __name__ == '__main__':
	main()