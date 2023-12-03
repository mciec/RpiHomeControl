ssh mciec@192.168.8.20 rm -f -r -d ~/projects/LedStripeWithSensors
ssh mciec@192.168.8.20 mkdir ~/projects
ssh mciec@192.168.8.20 mkdir ~/projects/LedStripeWithSensors
scp bin\Debug\net8.0\publish\linux-arm64\* mciec@192.168.8.20:~/projects/LedStripeWithSensors
$secrets = dotnet user-secrets --id 1c8133fb-92ec-49d1-8df3-ea9190fbddff list
$secretsCommand = "export DOTNET_LEDSTRIPEWITHSENSORS_" + $secrets.Replace(":", "__").Replace(" ", "")
Write-Host $secretsCommand
$sshRunCommand = $secretsCommand + " && cd ~/projects/LedStripeWithSensors && ~/.dotnet/dotnet LedStripeWithSensors.dll"
Write-Host $sshRunCommand
ssh mciec@192.168.8.20 $sshRunCommand
