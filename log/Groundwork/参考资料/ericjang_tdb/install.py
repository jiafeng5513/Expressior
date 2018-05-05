import notebook.nbextensions
import urllib
import zipfile
SOURCE_URL = 'https://github.com/ericjang/tdb/releases/download/tdb_ext_v0.1/tdb_ext.zip'
#urllib.urlretrieve(SOURCE_URL, 'tdb_ext.zip')#python2
urllib.request.urlretrieve(SOURCE_URL, 'tdb_ext.zip')
with zipfile.ZipFile('tdb_ext.zip', "r") as z:
    z.extractall("")
notebook.nbextensions.install_nbextension('tdb_ext',user=True)