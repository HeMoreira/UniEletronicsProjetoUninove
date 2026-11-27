/* ==========================================================================
   LÓGICA JAVASCRIPT: ORGANIZAÇÃO DE PEDIDOS
   Caminho: wwwroot/js/pedidos.js

   As linhas da tabela são geradas no servidor (Pedidos.cshtml) a partir dos
   produtos do banco de dados. Aqui fica apenas o filtro por status.
   ========================================================================== */

function filtrarPedidos(status) {
    // Atualiza classe ativa dos botões/links de filtro
    const filtros = document.querySelectorAll(".filtro");
    filtros.forEach(link => link.classList.remove("active-filter"));

    if (event && event.target) {
        event.target.classList.add("active-filter");
    }

    // Mostra apenas as linhas cujo data-status corresponde ao filtro escolhido
    const linhas = document.querySelectorAll("#tabela-produtos tr[data-status]");
    let visiveis = 0;

    linhas.forEach(linha => {
        const mostrar = status === "todos" || linha.dataset.status === status;
        linha.hidden = !mostrar;
        if (mostrar) visiveis++;
    });

    // Mensagem quando nenhum pedido corresponde ao filtro
    const linhaVazia = document.getElementById("linha-vazia");
    if (linhaVazia) {
        linhaVazia.hidden = visiveis > 0;
    }
}