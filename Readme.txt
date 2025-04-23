ESTRUÇÕES PARA INSTALAÇÃO DO SISTEMA DE SENHAS WebSenha

Certifique-se de colocar os arquivos do sistema WebSenha no caminho C:/

Antes de rodar o instalador cerifique das seguintes pré requisitos 

certifique-se que o computador tenhas as versão 8.0 do dotnet.

certifique-se que as portas 6766 e a porta 5000 estejam liberadas para saída no firewall da maquina.

certifique-se de ter o SqlServer instalado na maquina que vai ser o servidor .

Em caso de erro verifique se o dotnet e o SqlServer foram instalados corretamente 
você também pode conferir se as tabelas e o banco de dados foram criadores corretamente pelo SQL Server Management Studio 


fazer o desbloqueio do usuário sa do sqlserver, a senha definida para este usuário se encontra no KeeWeb.

Dentro de WebSenha localize o arquivo appsettings.json e substitua o nome do PC ou ' nome da sua maquina' pelo real nome do computador que esta instalando o sistema salve e feche o arquivo novamente.



PROCESSO PARA DEIXAR O SERVIDOR ONLINE

Para criar o arquivo /publish navegue ate a pasta onde se encontra o sistema pelo cmd como administrador
digite o comando 

dotnet publish -c Release -o ./publish

depois de criado  
Para iniciar o arquivo deve navegar ate a pasta publish que será criada dentro da pasta do sistema de senha WebSenha no cmd dentro da pasta /publish
digite o comando 

dotnet WebSenha.dll

após realizar toda a Configuração e necessário apenas colocar o bat  rodarWebSenha.bat para iniciar juntamente com Windows 




