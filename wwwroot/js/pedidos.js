/* ==========================================================================
   LÓGICA JAVASCRIPT: ORGANIZAÇÃO DE PEDIDOS
   Caminho: wwwroot/js/pedidos.js
   ========================================================================== */

// Caminho atualizado apontando para wwwroot/images/organizacao-servicos/
const products = [
    {
        imagem: "/images/organizacao-servicos/apple_watch_azul.png",
        produto: "Apple Watch",
        data: "12/10/2026",
        preco: "100.99",
        acao: "Enviado",
    },
    {
        imagem: "/images/organizacao-servicos/iphone_13_azu.png",
        produto: "Iphone 13",
        data: "12/10/2026",
        preco: "100.99",
        acao: "A caminho",
    },
    {
        imagem: "/images/organizacao-servicos/airpods_max_azul.png",
        produto: "Airpods",
        data: "12/10/2026",
        preco: "100.99",
        acao: "Recebido",
    },
    {
        imagem: "/images/organizacao-servicos/Samsung_Galaxy_Watch_azu.png",
        produto: "Samsung Galaxy Watch",
        data: "12/10/2026",
        preco: "100.99",
        acao: "Cancelado",
    }
];

document.addEventListener("DOMContentLoaded", function () {
    carregarTabela(products);
});

function getStatusClass(acao) {
    switch (acao) {
        case "Enviado": return "status-enviado";
        case "Cancelado": return "status-cancelado";
        case "A caminho": return "status-caminho";
        case "Recebido": return "status-recebido";
        default: return "";
    }
}

function carregarTabela(listaProdutos) {
    const tabela = document.getElementById("tabela-produtos");
    if (!tabela) return;

    tabela.innerHTML = "";

    listaProdutos.forEach(product => {
        const statusClass = getStatusClass(product.acao);

        tabela.innerHTML += `
        <tr>
            <td>
                <img src="${product.imagem}" alt="${product.produto}">
            </td>
            <td>${product.produto}</td>
            <td>${product.data}</td>
            <td><span class="product-data">R$ ${product.preco}</span></td>
            <td><span class="product-acao ${statusClass}">${product.acao}</span></td>
        </tr>
        `;
    });
}

function filtrarPedidos(status) {
    // Atualiza classe ativa dos botões/links de filtro
    const filtros = document.querySelectorAll(".filtro");
    filtros.forEach(link => link.classList.remove("active-filter"));

    if (event && event.target) {
        event.target.classList.add("active-filter");
    }

    if (status === "todos") {
        carregarTabela(products);
    } else {
        const produtosFiltrados = products.filter(p => p.acao === status);
        carregarTabela(produtosFiltrados);
    }
}