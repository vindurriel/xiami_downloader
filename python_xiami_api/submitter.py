#encoding=utf-8
#zip xiami player's output folder, put it in baidu cloud sync folder
import zipfile, sys,os,time
def cwd(*args):
	d=os.path.dirname(sys.argv[0])
	for x in args:
		d=os.path.join(d,x)
	return d
f_baidu=u"D:\\cloud\\百度云\\我的软件\\xiami"
f_xiami=os.path.realpath(cwd("..","Jean_Doe.Output"))
def push():
	#write version info, ID is unix time
	file(os.path.join(f_baidu,"version.txt"),'w').write(str(time.time()))
	z=zipfile.ZipFile(os.path.join(f_baidu,'latest.zip'),'w')
	def allows(x):
		if x=="access_token":
			return False
		ext=os.path.splitext(x.lower())[1]
		return ext not in ['.txt','.pdb','.log','.zip']
	for f in filter(allows,os.listdir(f_xiami)):
		z.write(os.path.join(f_xiami,f),f)
	z.close()
push()