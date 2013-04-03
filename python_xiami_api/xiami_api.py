#encoding=utf-8
'''usage: xiami_api.exe method_name file_input_args'''
import requests as r
import sys,os
client_id="55ee94348aeba2635326059e60d20a49"
client_secret="ec253979c7f51e10010a30241c2ca2de"
def cwd(*args):
	d=os.path.dirname(sys.argv[0])
	for x in args:
		d=os.path.join(d,x)
	return d
def md5(raw):
	import hashlib
	return hashlib.md5(raw).hexdigest() 
def get_new_token(username,password):
	url_new_token="http://api.xiami.com/api/oauth2/token"
	if len(password)!=32:
		password=md5(password)
	postObject={
	  "grant_type":"password",
	  "username":username,
	  "password":password,
	  "client_id":client_id,
	  "client_secret":client_secret,
	}
	resp=r.post(url_new_token,postObject)
	json=resp.json()
	if 'error' in json:
		die(json["error"])
	file(cwd("access_token"),'w').write(json["access_token"])
	return json["access_token"],json["refresh_token"]
def get_api_signature(dic,secret):
	res=""
	for k in sorted(dic.iterkeys()):
		res+=k+str(dic[k])
	res+=secret
	return md5(res)
def api_get(method,params={}):
	f=cwd("access_token")
	if not os.path.isfile(f):
		die("file access_token not found in %s. Get new token first."%cwd())
	access_token=file(f,'r').read()
	url_api="http://api.xiami.com/api"
	dic={
		"method":method,
		"api_key":client_id,
		"call_id":"1",
	}
	for k,v in params.iteritems():
		dic[k]=v
	dic["api_sig"]=get_api_signature(dic,client_secret)
	dic["access_token"]=access_token
	resp=r.get(url_api,params=dic)
	json=resp.json()
	if json['err']:
		die(json['err'])
	return json['data']
# access_token,refresh_token=get_new_token(username,password)
# dailysongs= api_get("Recommend.DailySongs")
# print len(dailysongs['data']['songs'])
def print_json(what):
	import json
	print json.dumps(what)
def die(why):
	print_json({"error":why})
	with file("xiami_api.log",'a') as f:
	 f.write(why)
	 f.write("\n")
	sys.exit(0)
if __name__ == '__main__':
	if len(sys.argv)<2:
		die(__doc__) 
	method=sys.argv[1]
	if "help" in method:
		die(__doc__) 
	args=sys.argv[2:]
	if method=="get_new_token":
		u,p=args
		access_token,refresh_token=get_new_token(u,p)
		print_json({"access_token":access_token,"refresh_token":refresh_token})
	elif method=="api_get":
		if len(args)==0:
			die("api method missing") 
		m=args[0]
		dic={}
		if len(args)>1:
			try:
				for x in args[1:]:
					k,v=x.split('=')
					dic[k]=v
			except Exception, e:
				die("api method %s args error: %s"%(m,args))
		res=api_get(m,dic)
		print_json(res)
	else:
		die("unknown method:"+method)