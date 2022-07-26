using Dataplace.Imersao.Core.Application.Orcamentos.ViewModels;
using dpLibrary05;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace Dataplace.Imersao.Core.Application.Orcamentos.Queries
{
    public class OrcamentoQueryHandler :
        IRequestHandler<ObterOcamentoQuery, OrcamentoViewModel>,
        IRequestHandler<ObterOrcamentoItemQuery, OrcamentoItemViewModel>
    {

        #region fields
        private readonly IDataAccess _dataAccess;
        private readonly Domain.Orcamentos.Repositories.IOrcamentoRepository _orcamentoRepository;
        #endregion

        #region contructors
        public OrcamentoQueryHandler(Domain.Orcamentos.Repositories.IOrcamentoRepository orcamentoRepository, IDataAccess dataAccess)
        {
            _orcamentoRepository = orcamentoRepository;
            _dataAccess = dataAccess;
        }
        #endregion
        public async Task<OrcamentoViewModel> Handle(ObterOcamentoQuery query, CancellationToken cancellationToken)
        {
     

            var sql = $@"
            SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
            SELECT 
	            Orcamento.CdEmpresa, 
		        Orcamento.CdFilial, 
		        Orcamento.NumOrcamento, 
		        Orcamento.CdCliente, 
		        Orcamento.DtOrcamento,
		        Orcamento.vlvendar as ValotTotal,
		        Orcamento.numdias as DiasValidade,
		        Orcamento.dtvalidade DataValidade,	
		        Orcamento.CdTabela,
		        Orcamento.SqTabela,
		        Orcamento.DtFechamento,
		        Orcamento.CdVendedor,
		        Orcamento.Usuario,
		        Orcamento.StOrcamento as Situacao
	        FROM Orcamento
            /**where**/	
            ";
            var builder = new SqlBuilder();
            var selector = builder.AddTemplate(sql);


            builder.Where("Orcamento.NumOrcamento = @NumOrcamento", new { query.NumOrcamento });
            var cmd = new CommandDefinition(selector.RawSql, selector.Parameters, flags: CommandFlags.NoCache);
            return _dataAccess.Connection.QueryFirstOrDefault<OrcamentoViewModel>(cmd);
        }

        public async Task<OrcamentoItemViewModel> Handle(ObterOrcamentoItemQuery query, CancellationToken cancellationToken)
        {
            var sql = $@"
            SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
            SELECT 
                OrcamentoItem.CdEmpresa,
                OrcamentoItem.CdFilial,
                OrcamentoItem.NumOrcamento,
                OrcamentoItem.Seq,
                OrcamentoItem.TpRegistro,
                OrcamentoItem.CdProduto,
                Produto.DsVenda as DsProduto,
                OrcamentoItem.qtdproduto as Quantidade,
                --OrcamentoItem.PrecoTabela,
                OrcamentoItem.PercAltPreco,
                OrcamentoItem.vlvenda as PrecoVenda,
                OrcamentoItem.vltotalcomopcionais as Total,
                OrcamentoItem.stitem as Status
	        FROM OrcamentoItem
            INNER JOIN Produto
		            ON Produto.CdProduto = OrcamentoItem.cdproduto
            /**where**/	
            ";
            var builder = new SqlBuilder();
            var selector = builder.AddTemplate(sql);


            builder.Where("OrcamentoItem.NumOrcamento = @NumOrcamento", new { query.NumOrcamento });
            builder.Where("OrcamentoItem.Seq = @NumOrcamento", new { query.Seq });

            var cmd = new CommandDefinition(selector.RawSql, selector.Parameters, flags: CommandFlags.NoCache);
            return _dataAccess.Connection.QueryFirstOrDefault<OrcamentoItemViewModel>(cmd);
        }


        //public async Task<OrcamentoViewModel> Handle(ObterOcamentoQuery request, CancellationToken cancellationToken)
        //{
        //    var cdEmpresa = "";
        //    var cdFilial = "";
        //    var orcamento = _orcamentoRepository.ObterOrcamento(cdEmpresa, cdFilial, request.NumOrcamento);
        //    return new OrcamentoViewModel()
        //    {
        //        NumOrcamento = orcamento.NumOrcamento,
        //        DtOrcamento = orcamento.DtOrcamento,
        //        DataValidade = orcamento.Validade.Data,
        //        DiasValidade = orcamento.Validade.Dias
        //    };
        //}

    }

}
