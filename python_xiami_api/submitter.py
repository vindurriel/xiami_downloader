#encoding=utf-8
#zip xiami player's output folder, upload it to baidu cloud using pcs api.
import sys,os,time,zipfile
def cwd(*args):
	d=os.path.dirname(sys.argv[0])
	for x in args:
		d=os.path.join(d,x)
	return d
f_xiami=os.path.realpath(cwd("..","Jean_Doe.Output"))
f_version=cwd("version.txt")
f_zip=cwd("latest.zip")
def refresh_token():
	url
def upload(path,fname):
	import requests as r
	url_file=  "https://pcs.baidu.com/rest/2.0/pcs/file"
	token="3.9f695526212e72665604185c73ae9a66.2592000.1385028974.4093755095-1182756"
	print "@uploading",repr(path)
	res=r.post(url_file,
		params={
			"method":"upload",
			"access_token":token,
			"path":path,
			"ondup":"overwrite",
		},
		files={
			"file":file(fname,'rb')
		}
	)
	print res.content
def push():
	#write version info, ID is unix time
	file(f_version,'w').write(str(time.time()))
	z=zipfile.ZipFile(f_zip,'w')
	def allows(x):
		if x=="access_token" or x=="latest":
			return False
		ext=os.path.splitext(x.lower())[1]
		return ext not in ['.txt','.pdb','.log','.zip']
	for f in filter(allows,os.listdir(f_xiami)):
		z.write(os.path.join(f_xiami,f),f)
	z.close()
	path="/apps/folder1/"
	upload(path+"version.txt",f_version)
	upload(path+"latest.zip",f_zip)
	os.remove(f_version)
	os.remove(f_zip)
push()