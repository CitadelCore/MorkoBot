#!/bin/bash
dotnet build -c release
rsync -a --progress ./bin/Release/ josephmarsden@teamdepot-ssh.chaosinitiative.com:/opt/morkobot/publish
ssh josephmarsden@teamdepot-ssh.chaosinitiative.com 'sudo supervisorctl restart morkobot'
