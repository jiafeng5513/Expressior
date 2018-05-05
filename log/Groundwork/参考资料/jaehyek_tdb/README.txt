1.解压tdb_py35
2.文件夹下运行:python setup.py install
3.把tdb_py35拷贝到D:\Libraries\Anaconda3\Lib\site-packages下
4.尝试import tdb,调整目录直到通过
5.python命令行下,运行:
    import notebook.nbextensions
    notebook.nbextensions.install_nbextension('tdb_ext',user=True)

    其中tdb_ext是tdb文件夹下的一个文件夹,目录是:"D:/Libraries/Anaconda3/Lib/site-packages/td/tdb_ext/"
6.到tdb的目录下,运行test.py,这样不能画图,但是能运行

7.把test.py的脚本内容放到jupyter里面运行,把最上面的注释内容也运行,可以实时画图

8.加载保存的脚本也可以