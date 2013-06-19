# -*- mode: python -*-
a = Analysis(['E:\\workspace\\xiami_downloader\\updater\\updater.py'],
             pathex=['e:\\tools\\Python27\\pyinstaller-2.0'],
             hiddenimports=[],
             hookspath=None)
pyz = PYZ(a.pure)
exe = EXE(pyz,
          a.scripts,
          a.binaries,
          a.zipfiles,
          a.datas,
          name='updater.exe',
          debug=False,
          strip=None,
          upx=True,
          console=False )
