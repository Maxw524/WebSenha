$(document).ready(function () {
    let tipoAtendimento = "A";  // Inicializa com "Automático"
    let tipoServico = "Caixa";  // Inicializa com "Caixa"
    let guiche = "";
    let senhaSelecionada = null;
    let cronometroInterval = null;
    let tempoDecorrido = 0;

    // Função para carregar senhas na fila de forma automática
    function carregarSenhasFila() {
        // Fazer a requisição para o endpoint com os parâmetros tipoAtendimento e tipoServico
        $.get(`/painels/GetSenhasFila?tipoAtendimento=${tipoAtendimento}&tipoServico=${tipoServico}`, function (data) {
            let senhasHtml = "";

            if (data.sucesso) {
                if (data.tipo === "Automático") {
                    // Exibir a contagem de senhas pendentes no modo automático
                    senhasHtml = `<p class="text-center">Total de senhas pendentes: <strong>${data.count}</strong></p>`;
                } else {
                    // Se não for automático, exibe a lista de senhas como antes
                    if (data.senhas && data.senhas.length > 0) {
                        // Filtrar senhas com base no tipo de serviço (Caixa ou Atendimento)
                        data.senhas.forEach(function (senha) {
                            if ((tipoServico === "Caixa" && senha.senha.startsWith("CX")) ||
                                (tipoServico === "Atendimento" && senha.senha.startsWith("AT"))) {
                                // Construir HTML para cada senha
                                senhasHtml += `
                                <div class="list-group-item d-flex justify-content-between align-items-center">
                                    <strong>${senha.senha}</strong>
                                    <span class="badge badge-${senha.status === 'Pendente' ? 'warning' : 'success'}">
                                        ${senha.status || 'Pendente'}
                                    </span>
                                </div>`;
                            }
                        });
                    } else {
                        // Caso não haja senhas, exibir mensagem
                        senhasHtml = '<p class="text-center">Não há senhas na fila.</p>';
                    }
                }
            } else {
                senhasHtml = `<p class="text-center text-danger">${data.mensagem || data.erro}</p>`;
            }

            // Atualizar o conteúdo da fila de senhas
            $('#senhasFila').html(senhasHtml);  // Substitui o conteúdo da fila com o novo HTML

        }).fail(function () {
            alert("Erro ao carregar as senhas.");
        });
    }

    // Configurar a atualização automática a cada 5 segundos (5000 milissegundos)
    setInterval(carregarSenhasFila, 5000); // A função será chamada a cada 5 segundos



   // Função para verificar o status da senha selecionada
function verificarStatusSenha() {
    if (!senhaSelecionada) return;
  
}


    // Chamar a função de verificação de status após qualquer operação
    $('#btnFinalizarAtendimento').click(function () {
        finalizarAtendimento();
        verificarStatusSenha(); // Verifica o status após finalizar o atendimento
    });

    $('#btnChamarNovamente').click(function () {
        if (!senhaSelecionada) {
            mostrarNotificacao('Erro', 'Nenhuma senha selecionada para chamar novamente.', 'danger');
            return;
        }

        // Obter o guichê preenchido
        const guicheInput = $('#guiche').val().trim();

        // Verificar se o guichê foi preenchido corretamente
        if (!guicheInput || !/^[0-9]+$/.test(guicheInput)) {
            mostrarNotificacao('Erro', 'Por favor, informe um número válido do guichê.', 'danger');
            return;
        }

        // A variável 'guiche' recebe o valor correto do campo de entrada
        guiche = guicheInput;

        // Preparar os dados para enviar ao servidor
        const dados = {
            Senha: senhaSelecionada, // A senha selecionada
            Guiche: guiche           // O guichê do campo de entrada
        };

        // Adiciona o indicador de carregamento ao botão e desabilita
        $('#btnChamarNovamente').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processando...');

        $.ajax({
            url: '/painels/chamar-novamente',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dados), // Envia a senha e o guichê para o backend
            success: function (response) {
                // Restaura o estado original do botão
                $('#btnChamarNovamente').prop('disabled', false).html('<i class="fas fa-sync-alt"></i> Chamar Novamente');

                if (response.sucesso) {
                    mostrarNotificacao('Sucesso', `Senha ${senhaSelecionada} chamada novamente com sucesso no guichê ${guiche}.`, 'success');
                    carregarSenhasFila(); // Atualizar a fila após chamar novamente
                } else {
                    mostrarNotificacao('Erro', response.erro || 'Erro ao chamar novamente a senha.', 'danger');
                }
            },
            error: function () {
                // Restaura o estado original do botão em caso de erro
                $('#btnChamarNovamente').prop('disabled', false).html('<i class="fas fa-sync-alt"></i> Chamar Novamente');
                mostrarNotificacao('Erro', 'Erro ao comunicar com o servidor.', 'danger');
            }
        });

        // Verificar status da senha após chamar novamente
        verificarStatusSenha();
    });


    // Função para verificar a senha selecionada periodicamente
    setInterval(verificarStatusSenha, 1000); // Verifica a cada 1 segundo, caso o status mude

    // Carregar senhas encaminhadas
    function carregarSenhasEncaminhadas() {
        console.log("Carregando senhas encaminhadas...");

        $.get('/painels/GetSenhasEncaminhadas', function (data) {
            console.log("Resposta da API /painels/GetSenhasEncaminhadas:", data);

            let senhasHtml = '';

            if (data && data.senhas && data.senhas.length > 0) {
                data.senhas.forEach(function (senha) {
                    // Aplica o filtro para as senhas encaminhadas com base no tipo de serviço
                    if ((tipoServico === "Caixa" && (senha.senha.startsWith("ATN") || senha.senha.startsWith("ATP"))) ||
                        (tipoServico === "Atendimento" && (senha.senha.startsWith("CXN") || senha.senha.startsWith("CXP")))) {
                        senhasHtml += `
                        <div class="list-group-item d-flex justify-content-between align-items-center" data-senha="${senha.senha}">
                            <strong>${senha.senha}</strong>
                        </div>`;
                    }
                });

                // Se houver senhas a serem exibidas, mostrar
                if (senhasHtml) {
                    $('#senhasEncaminhadasLista').html(senhasHtml);
                    $('#senhasEncaminhadasContainer').show(); // Exibe o container
                } else {
                    // Se não houver senhas válidas para o serviço, exibe uma mensagem
                    $('#senhasEncaminhadasLista').html('<p class="text-center text-muted">Nenhuma senha encaminhada no momento para este serviço.</p>');
                    $('#senhasEncaminhadasContainer').show(); // Exibe com mensagem vazia
                }
            } else {
                console.warn("Nenhuma senha encaminhada retornada pela API.");
                senhasHtml = '<p class="text-center text-muted">Nenhuma senha encaminhada no momento.</p>';
                $('#senhasEncaminhadasLista').html(senhasHtml);
                $('#senhasEncaminhadasContainer').show(); // Exibe com mensagem vazia
            }
        }).fail(function (error) {
            console.error("Erro ao carregar senhas encaminhadas:", error);
            mostrarNotificacao('Erro', 'Erro ao carregar senhas encaminhadas.', 'danger');
            $('#senhasEncaminhadasContainer').hide(); // Esconde o container em caso de falha
        });
    }

    // Função para configurar o polling
    function iniciarPolling() {
        // Polling para carregar senhas da fila e encaminhadas a cada 1 segundo (1000ms)
        setInterval(function () {
            carregarSenhasFila();
            carregarSenhasEncaminhadas();
        }, 1000);  // Intervalo de 1 segundo
    }

    // Iniciar o polling
    iniciarPolling();

    // Validar guichê (verificação de números)
    function validarGuiche() {
        guiche = $('#guiche').val().trim();
        const guicheRegex = /^[0-9]+$/; // Somente números

        if (!guiche || !guicheRegex.test(guiche)) {
            mostrarNotificacao('Erro', 'Por favor, informe um número válido do guichê.', 'danger');
            return false;
        }
        return true;
    }

    // Chamar próxima senha automaticamente alternando entre preferenciais e normais
    function chamarProximaSenha() {
        if (!validarGuiche()) return;

        let tipoSenha = "";

        if (tipoAtendimento === "A") {
            tipoSenha = tipoServico === "Caixa" ? "CXP" : "ATP"; // Senha preferencial
        } else {
            tipoSenha = tipoAtendimento === "P"
                ? (tipoServico === "Caixa" ? "CXP" : "ATP")  // Preferencial
                : (tipoServico === "Caixa" ? "CXN" : "ATN");  // Normal
        }

        $.post('/painels/chamar-proxima-senha', { tipoAtendimento, guiche, tipoServico }, function (response) {
            if (response.sucesso) {
                senhaSelecionada = response.ticket;
                $('#senhaSelecionada').text(response.ticket);
                $('#senhaSelecionadaContainer').show();
                carregarSenhasFila();
            } else {
                mostrarNotificacao('Erro', response.erro || "Erro ao chamar a próxima senha.", 'danger');
            }
        }).fail(function () {
            mostrarNotificacao('Erro', "Erro ao chamar a próxima senha.", 'danger');
        });
    }


    // Iniciar atendimento
    function iniciarAtendimento() {
        if (cronometroInterval) return;

        tempoDecorrido = 0;
        $('#cronometro').text(`Tempo decorrido: ${tempoDecorrido}s`).show();
        cronometroInterval = setInterval(function () {
            tempoDecorrido++;
            $('#cronometro').text(`Tempo decorrido: ${tempoDecorrido}s`);
        }, 1000);
        $('#btnIniciarAtendimento').prop('disabled', true);
    }

    // Finalizar atendimento
    function finalizarAtendimento() {
        if (!senhaSelecionada) return;

        clearInterval(cronometroInterval);
        $('#cronometro').hide();
        $('#btnIniciarAtendimento').prop('disabled', false);

        // Enviar a requisição com o corpo como JSON
        $.ajax({
            url: '/painels/finalizar-atendimento',
            type: 'POST',
            contentType: 'application/json',  // Definindo o Content-Type como JSON
            data: JSON.stringify({ senha: senhaSelecionada }),  // Convertendo o objeto para JSON
            success: function (response) {
                if (response.sucesso) {
                    mostrarNotificacao('Sucesso', `Atendimento da senha finalizado.`, 'success');
                    senhaSelecionada = null;
                    $('#senhaSelecionadaContainer').hide();
                    carregarSenhasFila();
                } else {
                    mostrarNotificacao('Erro', response.erro || "Erro ao finalizar o atendimento.", 'danger');
                }
            },
            error: function () {
                mostrarNotificacao('Erro', "Erro ao finalizar o atendimento.", 'danger');
            }
        });
    }

    // Chamar Senha Selecionada novamente
    $('#btnChamarNovamente').click(function () {
        if (!senhaSelecionada) {
            mostrarNotificacao('Erro', 'Nenhuma senha selecionada para chamar novamente.', 'danger');
            return;
        }

        const dados = {
            Senha: senhaSelecionada // Apenas a senha é enviada, sem alterar o guichê
        };

        // Adiciona o indicador de carregamento ao botão e desabilita
        $('#btnChamarNovamente').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processando...');

        $.ajax({
            url: '/painels/chamar-novamente',
            type: 'POST',
            contentType: 'application/json', // Certifique-se de que o backend aceita este formato
            data: JSON.stringify(dados),    // Enviando os dados como JSON
            success: function (response) {
                // Restaura o estado original do botão
                $('#btnChamarNovamente').prop('disabled', false).html('<i class="fas fa-sync-alt"></i> Chamar Novamente');

                if (response.sucesso) {
                    mostrarNotificacao('Sucesso', `Senha ${senhaSelecionada} chamada novamente com sucesso.`, 'success');
                    carregarSenhasFila(); // Atualizar a fila após chamar novamente
                } else {
                    mostrarNotificacao('Erro', response.erro || 'Erro ao chamar novamente a senha.', 'danger');
                }
            },
            error: function () {
                // Restaura o estado original do botão em caso de erro
                $('#btnChamarNovamente').prop('disabled', false).html('<i class="fas fa-sync-alt"></i> Chamar Novamente');
                mostrarNotificacao('Erro', 'Erro ao comunicar com o servidor.', 'danger');
            }
        });
    });
    // Não Compareceu
    $('#naoCompareceuBtn').click(function () {
        if (!senhaSelecionada) {
            mostrarNotificacao('Erro', 'Nenhuma senha selecionada para marcar como não compareceu.', 'danger');
            return;
        }

        const dados = {
            Senha: senhaSelecionada
        };

        // Exibir indicador de carregamento no botão
        $('#naoCompareceuBtn').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processando...');

        $.ajax({
            url: '/painels/nao-compareceu',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dados),    // Enviando os dados como JSON
            success: function (response) {
                // Restaurar o estado do botão
                $('#naoCompareceuBtn').prop('disabled', false).html('<i class="fas fa-times"></i> Não Compareceu');

                if (response.sucesso) {
                    mostrarNotificacao('Sucesso', `Senha ${senhaSelecionada} marcada como Não Compareceu.`, 'success');
                    carregarSenhasFila(); // Atualiza a fila após marcar como Não Compareceu
                    // Desmarcar a senha após marcar como Não Compareceu
                    senhaSelecionada = null;
                    $('#senhaSelecionada').text('');
                    $('#senhaSelecionadaContainer').hide();
                } else {
                    mostrarNotificacao('Erro', response.erro || 'Erro ao marcar a senha como Não Compareceu.', 'danger');
                }
            },
            error: function () {
                // Restaurar o estado do botão em caso de erro
                $('#naoCompareceuBtn').prop('disabled', false).html('<i class="fas fa-times"></i> Não Compareceu');
                mostrarNotificacao('Erro', 'Erro ao comunicar com o servidor.', 'danger');
            }
        });
    });


    // Mostrar notificação
    function mostrarNotificacao(titulo, mensagem, tipo) {
        $('#notification').removeClass('alert-info alert-success alert-danger')
            .addClass('alert-' + tipo)
            .html('<strong>' + titulo + ':</strong> ' + mensagem)
            .fadeIn();
        setTimeout(() => $('#notification').fadeOut(800), 5000);
    }

    // Função para excluir o serviço atual do modal de encaminhamento
    function carregarServicosDisponiveis() {
        const servicos = ["Caixa", "Atendimento"];  // Lista de serviços disponíveis
        const tipoServicoSelecionado = $("#tipoServico").val();
        let optionsHtml = "";

        servicos.forEach(function (servico) {
            if (servico !== tipoServicoSelecionado) {  // Não exibe o serviço selecionado
                optionsHtml += `<option value="${servico}">${servico}</option>`;
            }
        });

        // Atualizar o dropdown de serviços dentro do modal
        $("#servicoDestino").html(optionsHtml);
    }

    // Abrir modal para encaminhar senha
    $('#encaminharSenha').click(function () {
        carregarServicosDisponiveis();  // Carregar os serviços disponíveis antes de abrir o modal
        $('#encaminharSenhaModal').modal('show');
    });

    // Confirmar encaminhamento da senha
    $('#confirmarEncaminhamento').click(function () {
        const servicoDestino = $('#servicoDestino').val();  // Serviço selecionado no modal

        if (!servicoDestino) {
            mostrarNotificacao('Erro', 'Por favor, selecione um serviço para encaminhar a senha.', 'danger');
            return;
        }

        if (!senhaSelecionada) {
            mostrarNotificacao('Erro', 'Nenhuma senha selecionada para encaminhar.', 'danger');
            return;
        }

        // Verificar se a senha e o serviço destino estão definidos corretamente
        if (senhaSelecionada && servicoDestino) {
            $.ajax({
                url: '/painels/encaminhar-senha',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    Senha: senhaSelecionada,
                    DestinoServico: servicoDestino
                }),
                success: function (response) {
                    if (response.sucesso) {
                        mostrarNotificacao('Sucesso', `Senha ${senhaSelecionada} encaminhada para o serviço ${servicoDestino}.`, 'success');
                        $('#encaminharSenhaModal').modal('hide');
                        carregarSenhasFila();
                        // Desmarcar a senha da tela
                        senhaSelecionada = null;
                        $('#senhaSelecionada').text('');
                        $('#senhaSelecionadaContainer').hide();
                    } else {
                        mostrarNotificacao('Erro', response.erro || "Erro ao encaminhar a senha.", 'danger');
                    }
                },
                error: function () {
                    mostrarNotificacao('Erro', "Erro ao encaminhar a senha.", 'danger');
                }
            });
        } else {
            mostrarNotificacao('Erro', "Há um erro nos dados enviados para encaminhar a senha.", 'danger');
        }
    });

    // Interação com os botões e seletores
    $('#chamarSenhaBtn').click(chamarProximaSenha);
    $('#btnIniciarAtendimento').click(iniciarAtendimento);
    $('#btnFinalizarAtendimento').click(finalizarAtendimento);
    $('#tipoAtendimento').change(function () {
        tipoAtendimento = $(this).val();
        carregarSenhasFila();
    });
    $('#tipoServico').change(function () {
        tipoServico = $(this).val();
        carregarSenhasFila();
    });

    // Ao clicar em uma senha encaminhada
    $('#senhasEncaminhadasLista').on('click', '.list-group-item', function () {
        senhaSelecionada = $(this).find('strong').text(); // Captura a senha clicada
        const guicheInput = $('#guiche').val(); // Captura o valor do guichê

        if (!guicheInput || guicheInput.trim() === '') {
            mostrarNotificacao('Erro', 'Por favor, informe o número do guichê antes de chamar a senha.', 'danger');
            return;
        }

        guiche = guicheInput.trim(); // Garantimos que o guichê foi preenchido

        // Verificação do guichê
        if (!validarGuiche()) {
            return;
        }

        const dados = {
            senha: senhaSelecionada,
            tipoAtendimento: tipoAtendimento,
            guiche: guiche,  // Envia o guichê correto
            status: 1        // Define o status como "Chamado"
        };

        $.ajax({
            url: '/painels/chamar-novamente-encaminhada',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dados),
            success: function (response) {
                if (response.sucesso) {
                    mostrarNotificacao('Sucesso', `Senha ${senhaSelecionada} chamada novamente no guichê ${guiche}.`, 'success');
                    carregarSenhasFila();
                    carregarSenhasEncaminhadas();
                } else {
                    mostrarNotificacao('Erro', response.erro || 'Erro ao chamar novamente a senha.', 'danger');
                }
            },
            error: function () {
                mostrarNotificacao('Erro', 'Erro ao comunicar com o servidor.', 'danger');
            }
        });
    });

    // Inicializa a página carregando as senhas
    carregarSenhasFila();
    carregarSenhasEncaminhadas();  // Carregar também as senhas encaminhadas ao carregar a página
});
