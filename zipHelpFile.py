import os
import zipfile 

#remove current help 
os.remove("OptimumDynamicsHelp.zip")

def zipdir(path, ziph):
    # ziph is zipfile handle
    for root, dirs, files in os.walk(path):
        for file in files:
            ziph.write(os.path.join(root, file))

zipf = zipfile.ZipFile('OptimumDynamicsHelp.zip', 'w', zipfile.ZIP_DEFLATED)
zipdir('site/', zipf)
zipf.close()


