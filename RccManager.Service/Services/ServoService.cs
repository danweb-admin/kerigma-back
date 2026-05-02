using System;
using System.Net;
using AutoMapper;
using Newtonsoft.Json;
using RccManager.Domain.Dtos.DecanatoSetor;
using RccManager.Domain.Dtos.ParoquiaCapela;
using RccManager.Domain.Dtos.Servo;
using RccManager.Domain.Dtos.Users;
using RccManager.Domain.Entities;
using RccManager.Domain.Exception.Decanato;
using RccManager.Domain.Exception.Servo;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Domain.Interfaces.Services;
using RccManager.Domain.Responses;
using RccManager.Service.Helper;
using static StackExchange.Redis.Role;

namespace RccManager.Service.Services
{
	public class ServoService : IServoService
	{
        private readonly IMapper _mapper;
        private readonly IServoRepository _repository;
        private readonly IServoTempRepository _repositoryTemp;



        public ServoService(IMapper mapper, IServoRepository repository, IServoTempRepository repositoryTemp)
        {
            _mapper = mapper;
            _repository = repository;
            _repositoryTemp = repositoryTemp;
        }

        public async Task<HttpResponse> Create(ServoDto servo)
        {
            servo.Birthday = Utils.formatDate(servo.Birthday1);
            var servo_ = _mapper.Map<Servo>(servo);

            var exists = await _repository.GetByCPF(Utils.Encrypt(servo.Cpf));

            if (exists)
                throw new ValidateByCpfOrEmailException("Este CPF já está sendo utilizado.");

            exists = await _repository.GetByEmail(Utils.Encrypt(servo.Email));

            if (exists)
                throw new ValidateByCpfOrEmailException("Este EMAIL já está sendo utilizado.");

            var result = await _repository.Insert(servo_);

            if (result == null)
                return new HttpResponse { Message = "Houve um problema para criar Servo(a)", StatusCode = (int)HttpStatusCode.BadRequest };

            return new HttpResponse { Message = "Servo(a) criado com sucesso.", StatusCode = (int)HttpStatusCode.OK };
        }

        public async  Task DescriptografarServos()
        {
           
            var servos = await _repositoryTemp.GetAll();

            foreach (var servo in servos)
            {
                try
                {
                    servo.NamePlain = Utils.Decrypt(servo.Name);
                    servo.CpfPlain = Utils.Decrypt(servo.Cpf).Replace(".","").Replace("-","");
                    servo.EmailPlain = Utils.Decrypt(servo.Email);
                    servo.CellphonePlain = Utils.Decrypt(servo.CellPhone);

                    await _repositoryTemp.Update(servo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
 
        }

        public async Task<IEnumerable<ServoDtoResult>> GetAll(Guid grupoOracaoId)
        {
            var list = _mapper.Map<IEnumerable<ServoDtoResult>>(await _repository.GetAll(grupoOracaoId));
            return list.OrderBy(x => x.Name);

        }

        public async Task<IEnumerable<ServoDtoResult>> GetAll(string search)
        {
            var result = _mapper.Map<IEnumerable<ServoDtoResult>>(await _repository.GetAll());

            return result.Where(x => x.Name.Contains(search));

        }

        public async Task<ServoDtoResult> GetByCPF(string cpf)
        {
            var cpf_ = Utils.Encrypt(cpf);

            var servo = await _repository.GetServoByCPF(cpf_);


            if (servo == null)
            {
                cpf_ = Utils.Encrypt(cpf.Replace(".","").Replace("-",""));

                servo = await _repository.GetServoByCPF(cpf_);
            }

            return _mapper.Map<ServoDtoResult>(servo);
        }

        public async Task<HttpResponse> Update(ServoDto servo, Guid id)
        {
            servo.Birthday = Utils.formatDate(servo.Birthday1);
            var exists = await _repository.GetByCPF(id ,Utils.Encrypt(servo.Cpf));

            if (exists)
                throw new ValidateByCpfOrEmailException("Este CPF já está sendo utilizado.");

            var servo_ = _mapper.Map<Servo>(servo);
            servo_.Id = id;

            servo_.CellphonePlain = Utils.SomenteNumeros(servo.CellPhone);
            servo_.CpfPlain = Utils.SomenteNumeros(servo.Cpf);
            servo_.EmailPlain = servo.Email;
            servo_.NamePlain = servo.Name;

            var result = await _repository.Update(servo_);

            if (result == null)
                return new HttpResponse { Message = "Houve um problema para atualizar o objeto", StatusCode = (int)HttpStatusCode.BadRequest };

            return new HttpResponse { Message = "Servo(a) atualizado(a) com sucesso.", StatusCode = (int)HttpStatusCode.OK };
        }
    }
}

