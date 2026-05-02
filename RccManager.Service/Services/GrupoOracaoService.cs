using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using RccManager.Domain.Dtos;
using RccManager.Domain.Dtos.GrupoOracao;
using RccManager.Domain.Dtos.ParoquiaCapela;
using RccManager.Domain.Dtos.Users;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Domain.Interfaces.Services;
using RccManager.Domain.Responses;
using RccManager.Service.Enum;
using RccManager.Service.Helper;

namespace RccManager.Service.Services
{
	public class GrupoOracaoService : IGrupoOracaoService
	{
        private readonly IMapper mapper;
        private readonly IGrupoOracaoRepository repository;
        private readonly IServoTempRepository servoTempRepository;
        private readonly IUserRepository userRepository;

        private readonly IHistoryRepository history;

        public GrupoOracaoService(IMapper _mapper, IGrupoOracaoRepository _repository, IHistoryRepository _history, IServoTempRepository _servoTempRepository, IUserRepository _userRepository)
        {
            mapper = _mapper;
            repository = _repository;
            servoTempRepository = _servoTempRepository;
            history = _history;
            userRepository = _userRepository;
        }

        public async Task<HttpResponse> Create(GrupoOracaoDto grupoOracao)
        {

            grupoOracao.FoundationDate = Utils.formatDate(grupoOracao.FoundationDate1);
            grupoOracao.Time = Utils.formaTime(grupoOracao.Time1);

            var grupoOracao_ = mapper.Map<GrupoOracao>(grupoOracao);
            var result = await repository.Insert(grupoOracao_);

            if (result == null)
                return new HttpResponse { Message = "Houve um problema para criar Grupo de Oração", StatusCode = (int)HttpStatusCode.BadRequest };

            // adiciona a tabela de histórico de alteracao
            await history.Add(TableEnum.GrupoOracao.ToString(), result.Id, OperationEnum.Criacao.ToString());

            return new HttpResponse { Message = "Grupo de Oração criado com sucesso.", StatusCode = (int)HttpStatusCode.OK };
        }

        public async Task<IEnumerable<GrupoOracaoDtoResult>> GetAll(string search, UserDtoResult user)
        {
            var userEntity = mapper.Map<User>(user);
            var entities = await repository.GetAll(search, userEntity);

            var result = mapper.Map<IEnumerable<GrupoOracaoDtoResult>>(entities);

            if (user.Name != "administrador")
            {
            foreach (var item in result)
                item.CsvUrl = string.Empty;
            }

            foreach (var grupo in result)
            {
            if (grupo.ServosTemp != null)
            {
                grupo.ServosTemp = grupo.ServosTemp
                    .Where(s => s.Active == true)
                    .ToList();
            }
            }

            return result;
        }


        public async Task<IEnumerable<GrupoOracaoDtoResult>> GetAll()
        {
            var entities = await repository.GetAll();

            foreach (var item in entities)
                item.CsvUrl = string.Empty;

            return mapper.Map<IEnumerable<GrupoOracaoDtoResult>>(entities); 
        }

        public async Task<HttpResponse> ImportCSV(Guid grupoOracaoId, UserDtoResult user)
        {
            var url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQSRbikz2NWNIiNZY9rzEkKw9bjTuioP8NGUvkTmdlZQl57IRP1z1ym8tFRC8xpvKnGe1C_OX_ypLTq/pub?output=csv";


            using (HttpClient client = new HttpClient())
            {
                // Baixa o conteúdo do CSV
                var csvData = await client.GetStringAsync(url);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    BadDataFound = null,
                };

                // Lê o CSV e processa
                using (var reader1 = new StringReader(csvData))
                using (var csv = new CsvReader(reader1, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader(); // Lê o cabeçalho (nomes das colunas)

                    while (csv.Read())
                    {

                        
                        var id = csv.GetField("Id");
                        var grupoOracao = csv.GetField("GrupoOracao");
                        var decanato = csv.GetField("Decanato");
                        var planilha = csv.GetField("Planilha");

                        if (string.IsNullOrEmpty(planilha))
                            continue;

                        if (grupoOracaoId != Guid.Parse(id))
                            continue;

                        try
                        {
                            await ImportCSV(planilha, Guid.Parse(id), grupoOracao, decanato);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Grupo Oracao: {grupoOracao}, Decanato: {decanato}");
                            Console.WriteLine(ex.Message);

                        }
                        

                    }
                }
            }

            return new HttpResponse { Message = "Servos Temporário importado com sucesso.", StatusCode = (int)HttpStatusCode.OK };
        }

        public async Task<HttpResponse> ImportCSV(UserDtoResult user)
        {
            
            var url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQSRbikz2NWNIiNZY9rzEkKw9bjTuioP8NGUvkTmdlZQl57IRP1z1ym8tFRC8xpvKnGe1C_OX_ypLTq/pub?output=csv";


            using (HttpClient client = new HttpClient())
            {
                // Baixa o conteúdo do CSV
                var csvData = await client.GetStringAsync(url);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    BadDataFound = null,
                };

                // Lê o CSV e processa
                using (var reader1 = new StringReader(csvData))
                using (var csv = new CsvReader(reader1, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader(); // Lê o cabeçalho (nomes das colunas)

                    while (csv.Read())
                    {

                        
                        var id = csv.GetField("Id");
                        var grupoOracao = csv.GetField("GrupoOracao");
                        var decanato = csv.GetField("Decanato");
                        var planilha = csv.GetField("Planilha");

                        if (string.IsNullOrEmpty(planilha))
                            continue;
                        try
                        {
                            await ImportCSV(planilha, Guid.Parse(id), grupoOracao, decanato);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Grupo Oracao: {grupoOracao}, Decanato: {decanato}");
                            Console.WriteLine(ex.Message);

                        }
                        

                    }
                }
            }

            return new HttpResponse { Message = "Servos Temporário importado com sucesso.", StatusCode = (int)HttpStatusCode.OK };

        }

        public async Task<HttpResponse> ImportCSV(string csvUrl, Guid id, string grupoOracao, string decanato)
        {
            var list = await servoTempRepository.GetAll(id);

            using (HttpClient client = new HttpClient())
            {
                // Baixa o conteúdo do CSV
                var csvData = await client.GetStringAsync(csvUrl);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    BadDataFound = null,
                };

                // Lê o CSV e processa
                using (var reader1 = new StringReader(csvData))
                using (var csv = new CsvReader(reader1, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader(); // Lê o cabeçalho (nomes das colunas)

                    while (csv.Read())
                    {

                        var createdAt = csv.GetField("Carimbo de data/hora");
                        var name = csv.GetField("Nome");
                        var email = csv.GetField("Email");
                        var cpf = csv.GetField<string>(10);
                        var phone = csv.GetField("Celular/WhatsApp (DD) 99000-9999");
                        var birthdate = csv.GetField("Data de Nascimento");
                        var mainMinistry = csv.GetField("Ministério Principal");
                        var secondaryMinistry = csv.GetField("Ministério Principal");

                        cpf = cpf.Replace(".", "").Replace("-", "").Trim();

                        var result = list.Where(x => x.Cpf == Utils.Encrypt(cpf) && x.GrupoOracaoId == id);


                        if (!result.Any())
                        {
                            var servoTemp = new ServoTemp
                            {
                                Active = true,
                                CreatedAt = Utils.formatDateTime(createdAt),
                                Name = Utils.Encrypt(name),
                                Email = Utils.Encrypt(email),
                                Cpf = Utils.Encrypt(cpf),
                                CellPhone = Utils.Encrypt(phone),
                                Birthday = birthdate,
                                Checked = false,
                                GrupoOracaoId = id,
                                MainMinistry = Ministerios.returnMinistryValue(mainMinistry),
                                SecondaryMinistry = Ministerios.returnMinistryValue(secondaryMinistry)
                            };

                            try
                            {
                                await servoTempRepository.Insert(servoTemp);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Servo: {name}, GrupoOracao: {grupoOracao}, Decanato: {decanato}");
                                //Console.WriteLine(ex.Message);
                            }
                            
                            
                           
                        }
                    }
                }
            }

       
            return new HttpResponse { Message = "Servos Temporário importado com sucesso.", StatusCode = (int)HttpStatusCode.OK };
        }

        public async Task<HttpResponse> ImportCSV()
        {
            try
            {
                var user = await userRepository.GetByEmail("admin@admin.com");
                var user_ = mapper.Map<UserDtoResult>(user);

                await ImportCSV(user_);

                return new HttpResponse { Message = "Servos Temporário importado com sucesso.", StatusCode = (int)HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            } 
 
        }

        public async Task<HttpResponse> Update(GrupoOracaoDto grupoOracao, Guid id)
        {
            var grupoOracao_ = mapper.Map<GrupoOracao>(grupoOracao);
            grupoOracao_.Id = id;
            grupoOracao_.FoundationDate = Utils.formatDate(grupoOracao.FoundationDate1);
            grupoOracao_.Time = Utils.formaTime(grupoOracao.Time1);

            var result = await repository.Update(grupoOracao_);

            if (result == null)
                return new HttpResponse { Message = "Houve um problema para atualizar o objeto", StatusCode = (int)HttpStatusCode.BadRequest };

            // adiciona a tabela de histórico de alteracao
            await history.Add(TableEnum.GrupoOracao.ToString(), result.Id, OperationEnum.Alteracao.ToString());

            return new HttpResponse { Message = "Grupo de Oração atualizado com sucesso.", StatusCode = (int)HttpStatusCode.OK };
        }
        
    }
}

