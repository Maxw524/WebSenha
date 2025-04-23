$(document).ready(function () {
    var maxUltimasSenhas = 3; // Número máximo de senhas a exibir na lista das últimas chamadas
    var listaUltimasSenhas = [];

    // Função para carregar mídia (imagens ou vídeos)
    function carregarConteudo() {
        $.ajax({
            url: '/Home/GetMediaFiles', // URL para buscar os arquivos
            type: 'GET',
            success: function (data) {
                console.log("Conteúdo de mídia carregado:", data);

                if (data.length > 0) {
                    var randomFile = data[Math.floor(Math.random() * data.length)];
                    var fileExtension = randomFile.split('.').pop().toLowerCase();

                    // Verifica se o arquivo é um vídeo
                    if (fileExtension === 'mp4') {
                        $('#video-publicidade').html(
                            '<video class="media" controls autoplay loop muted>' +
                            '<source src="' + randomFile + '" type="video/mp4">' +
                            'Seu navegador não suporta o elemento de vídeo.' +
                            '</video>'
                        );
                    }
                    // Verifica se o arquivo é uma imagem
                    else if (['jpg', 'jpeg', 'png', 'gif'].includes(fileExtension)) {
                        $('#video-publicidade').html('<img class="media" src="' + randomFile + '" alt="Imagem exemplo">');
                    }

                    // Após carregar, aplica a classe 'active' no vídeo ou imagem
                    $('.media').on('load', function () {
                        $(this).addClass('active');
                    });
                } else {
                    $('#video-publicidade').html('<p>Não há arquivos disponíveis.</p>');
                }
            },
            error: function (xhr, status, error) {
                console.error("Erro ao carregar a mídia:", error);
                $('#video-publicidade').html('<p>Erro ao carregar o conteúdo. Tente novamente mais tarde.</p>');
            }
        });
    }

    // Função para atualizar as últimas senhas chamadas (apenas status = 1)
    function atualizarSenhas() {
        var $senhasChamadas = $('#senhasChamadas');
        $senhasChamadas.empty();

        listaUltimasSenhas.forEach(function (senha) {
            // Só exibe senhas com status = 1 (Chamado)
            if (senha.status === "Chamado" || senha.status === 1) {
                var tipoFormatado = senha.tipo === 'P' ? 'Preferencial' : 'Normal';

                var novaSenhaElemento = $('<div class="senha-chamada">' +
                    '<p> ' + senha.senha + '</p>' +
                    '<p>Guichê: ' + senha.guiche + '</p>' +
                    '</div>');

                $senhasChamadas.prepend(novaSenhaElemento);

                setTimeout(function () {
                    novaSenhaElemento.addClass('visible');
                }, 100);
            }
        });
    }

    // Função para exibir a próxima senha chamada no painel principal
    function exibirProximaSenhaChamada(senha) {
        // Só exibe senhas com status = 1 (Chamado) ou "Chamado"
        if (senha.status === "Chamado" || senha.status === 1) {
            var tipoFormatado = senha.tipo === 'P' ? 'Preferencial' : 'Normal';

            $('#senha').html('<p>' + senha.senha + '</p>');
            $('#guiche').html('<p>' + senha.guiche + '</p>'); // Exibe guiche como número
            $('#tipo-senha').text(tipoFormatado);

            // Tocar áudio com múltiplos formatos
            var alertaAudio = new Audio('/assets/sounds/alerta.mp3'); // Caminho para o áudio

            // Verificar se o arquivo MP3 é suportado
            alertaAudio.oncanplaythrough = function () {
                alertaAudio.play().then(() => {
                    console.log("Som de alerta tocado com sucesso!");
                }).catch(error => {
                    console.error("Erro ao tentar tocar o áudio:", error);
                });
            };

            alertaAudio.onerror = function () {
                // Tenta outros formatos caso o MP3 não seja suportado
                console.error("Erro ao carregar o áudio MP3. Tentando formatos alternativos.");

                var outrosFormatos = [
                    '/assets/sounds/alerta.ogg',
                    '/assets/sounds/alerta.wav'
                ];

                for (var i = 0; i < outrosFormatos.length; i++) {
                    var alternativaAudio = new Audio(outrosFormatos[i]);
                    alternativaAudio.play().then(() => {
                        console.log("Som de alerta tocado com sucesso (formato alternativo)!");
                    }).catch(error => {
                        console.error("Erro ao tentar tocar o áudio (formato alternativo):", error);
                    });
                }
            };
        } else {
            console.warn("Senha não chamada, status diferente de 1 ou 'Chamado':", senha);
        }
    }

    // Função para buscar a última senha chamada do backend
    function carregarUltimaSenha() {
        $.ajax({
            url: '/Home/GetUltimaSenha',
            type: 'GET',
            success: function (data) {
                if (!data || !data.senha) {
                    console.warn("Nenhuma senha disponível.");
                    return;
                }

                var novaSenha = {
                    senha: data.senha,
                    guiche: data.guiche && !isNaN(data.guiche) ? parseInt(data.guiche) : "Desconhecido", // Garante que guiche seja um número
                    tipo: data.tipo,
                    status: data.status // Mantém o status como estava
                };

                console.log("Última senha do backend: ", novaSenha);

                // Exibe a próxima senha no painel principal
                exibirProximaSenhaChamada(novaSenha);

                // Atualiza a lista de últimas senhas chamadas (apenas status = 1 ou 'Chamado')
                if (listaUltimasSenhas.length >= maxUltimasSenhas) {
                    listaUltimasSenhas.pop(); // Remove o último item da lista
                }
                listaUltimasSenhas.unshift(novaSenha); // Adiciona a nova senha no início da lista
                atualizarSenhas();
            },
            error: function (xhr, status, error) {
                console.error("Erro ao carregar a última senha:", error);
            }
        });
    }

    // Configuração do SignalR
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/senhaHub")
        .build();

    connection.on("ReceberSenhaAtualizada", function (senha, tipo, guiche, status) {
        console.log("Dados recebidos via SignalR:", senha, tipo, guiche, status);

        var novaSenha = {};

        // Atribuindo os valores corretamente
        novaSenha.senha = senha;
        novaSenha.tipo = tipo || "N"; // Garantindo que o tipo seja atribuído corretamente
        novaSenha.guiche = !isNaN(guiche) ? guiche : "Desconhecido"; // Garantindo que guiche seja um número
        novaSenha.status = status || "Chamado"; // Mantém o status como estava (caso seja "Chamado")

        console.log("Nova senha mapeada:", novaSenha);

        if (novaSenha.status === "Chamado" || novaSenha.status === 1) {
            if (listaUltimasSenhas.length >= maxUltimasSenhas) {
                listaUltimasSenhas.pop();
            }
            listaUltimasSenhas.unshift(novaSenha);
            exibirProximaSenhaChamada(novaSenha);
            atualizarSenhas();
        } else {
            console.log("A senha não foi chamada (status diferente de 'Chamado').", novaSenha);
        }
    });

    connection.start().catch(function (err) {
        console.error('Erro ao conectar no SignalR: ', err.toString());
        setTimeout(() => connection.start(), 5000); // Tenta reconectar após 5 segundos
    });

    // Carrega a última senha ao carregar a página
    carregarUltimaSenha();

    // Carrega o conteúdo de mídia inicialmente
    carregarConteudo();
    setInterval(carregarConteudo, 20000); // Atualiza a mídia a cada 20 segundos

    // Função para tocar o áudio quando a logo for clicada
    $('#logo-sicoob').click(function () {
        var alertaAudio = new Audio('/assets/sounds/alerta.mp3'); // Caminho para o áudio
        alertaAudio.play().then(() => {
            console.log("Som de alerta tocado com sucesso!");
        }).catch(error => {
            console.error("Erro ao tentar tocar o áudio:", error);
        });
    });
});
