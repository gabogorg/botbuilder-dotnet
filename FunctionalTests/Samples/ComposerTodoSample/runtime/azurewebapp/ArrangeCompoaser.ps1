// This script arranges the composer files into the runtime project
// so we can do a webdeploy of the project from azure

Remove-Item 'ComposerDialog' -Recurse

New-Item -Path 'ComposerDialog/dialogs' -ItemType Directory
copy-item '../../dialogs/*' 'ComposerDialog/dialogs' -force -recurse -verbose

New-Item -Path 'ComposerDialog/knowledge-base' -ItemType Directory
copy-item '../../knowledge-base/*' 'ComposerDialog/knowledge-base' -force -recurse -verbose

New-Item -Path 'ComposerDialog/language-generation' -ItemType Directory
copy-item '../../language-generation/*' 'ComposerDialog/language-generation' -force -recurse -verbose

New-Item -Path 'ComposerDialog/language-understanding' -ItemType Directory
copy-item '../../language-understanding/*' 'ComposerDialog/language-understanding' -force -recurse -verbose

New-Item -Path 'ComposerDialog/schemas' -ItemType Directory
copy-item '../../schemas/**/*.schema' 'ComposerDialog/schemas' -force -recurse -verbose

New-Item -Path 'ComposerDialog/settings' -ItemType Directory
copy-item '../../settings/*' 'ComposerDialog/settings' -force -recurse -verbose

copy-item '../../*.dialog' 'ComposerDialog/' -force -recurse -verbose 