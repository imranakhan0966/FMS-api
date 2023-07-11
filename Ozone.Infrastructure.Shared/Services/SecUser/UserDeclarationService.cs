﻿using Ozone.Application.DTOs;
using Ozone.Application.Interfaces.Setup;
using Ozone.Application.Repository;
using Ozone.Infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using AutoMapper;
using Ozone.Application;
using Ozone.Application.Interfaces;
using System.Linq;

namespace Ozone.Infrastructure.Shared.Services
{
   public class UserDeclarationService : GenericRepositoryAsync<UserDeclaration>, IUserDeclarationService
    {  private readonly OzoneContext _dbContext;
    //  private readonly DbSet<Library> _user;
    private readonly IMapper _mapper;
    private IUserSessionHelper _userSession;
    // private IDataShapeHelper<Library> _dataShaper;
    private readonly IUnitOfWork _unitOfWork;

    public UserDeclarationService(
         IUnitOfWork unitOfWork,
     OzoneContext dbContext,
    //IDataShapeHelper<Library> dataShaper,
    IMapper mapper,
    IUserSessionHelper userSession) : base(dbContext)
    {
        this._unitOfWork = unitOfWork;
        _dbContext = dbContext;
        //  _user = dbContext.Set<Library>();
        //_dataShaper = dataShaper;
        this._mapper = mapper;
        this._userSession = userSession;
        //_mockData = mockData;
    }
    public async Task<string> Create(UserDeclarationModel input)
    {
        using (var transaction = _unitOfWork.BeginTransaction())
        {
            UserDeclaration DbResult = await Task.Run(() => _dbContext.UserDeclaration.Where(x => x.Id == input.Id).FirstOrDefault());

            //string password = _secPolicyRepo.GetPasswordComplexityRegexPolicy().ToString();


            try
            {
                var message = "";
                long newid;
                bool New = false;
                if (DbResult == null)
                {
                    New = true;
                    DbResult = new UserDeclaration();
                }
                DbResult.UserId = input.UserId;
                DbResult.CompanyName = input.CompanyName;
                DbResult.ContractTypeId = input.ContractTypeId;
                DbResult.Interest = input.Interest;
                //DbResult.ApprovalStatusId = input.ApprovalStatusId;

                DbResult.StartYear = input.StartYear;
                DbResult.EndYear = input.EndYear;
              

                if (New == true)
                    {
                        DbResult.ApprovalStatusId = 1;
                        DbResult.CreatedBy = input.CreatedBy;
                    DbResult.CreatedDate = input.CreatedDate;
                    DbResult.IsDeleted = false;
                        DbResult.IsActive = true;
                        await base.AddAsync(DbResult);
                    message = "Successfully Inserted!";
                }
                else
                    {
                        DbResult.ApprovalStatusId = 10002;
                        //DbResult.LastModifiedBy = input.LastModifiedBy;
                        //DbResult.LastModifiedDate = input.LastModifiedDate;
                        await base.UpdateAsync(DbResult);
                    message = "Successfully Updated!";
                }
                //await base.AddAsync(secuserEntity);
                //await SecUser.(secuserEntity);
                // _dbContext.SecUser.Add(secuserEntity);
                //  ozonedb.Add
                //  await _secuserRepository.CreateUser(secuserEntity);
                // await _unitOfWork.SaveChangesAsync();
                var result = await _unitOfWork.SaveChangesAsync();



                transaction.Commit();
                return message;

            }

            catch
            {
                transaction.Rollback();
                return "Not Inserted!";
            }



        }

    }

    public async Task<UserDeclarationModel> GetUserDeclarationBYId(long id)
    {
        var result = new UserDeclarationModel();
        var Dbresult = await Task.Run(() => _dbContext.UserDeclaration.Where(x => x.Id == id).FirstOrDefault());
        result = _mapper.Map<UserDeclarationModel>(Dbresult);
        return result;
    }

    private List<UserDeclarationModel> GetPage(List<UserDeclarationModel> list, int page, int pageSize)
    {
        return list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }
    public async Task<GetPagedUserDeclarationModel> GetPagedUserDeclarationResponse(PagedResponseModel model)
    {
        try
        {

            var result = new GetPagedUserDeclarationModel();
            var UserStdList = new List<UserDeclarationModel>();

            //if (model.AuthAllowed == true)
            //{
            //var list = await _dbContext.Certification.Where(x => x.IsDeleted == false && x.IsActive == true &&
            //              (x.Module.Name.ToLower().Contains(model.Keyword.ToLower()) ||
            //             x.ModuleId.ToString().ToLower().Contains(model.Keyword.ToLower()))).OrderByDescending(x => x.Id).ToListAsync();

            var list = await _dbContext.UserDeclaration.Include("ContractType").Include("ApprovalStatus").Where(x => x.IsDeleted == false && x.UserId.ToString()==model.Keyword ).ToListAsync();
            UserStdList = _mapper.Map<List<UserDeclarationModel>>(list);
            // return list;
            //}


            //else
            //{
            //    var list = await _dbContext.Certification.Include("Module").Include("Status").Where(x => x.IsDeleted == false && x.IsActive == true &&
            //                 (x.Module.Name.ToLower().Contains(model.Keyword.ToLower()) ||
            //                x.Code.ToLower().Contains(model.Keyword.ToLower()))).OrderByDescending(x => x.Id).ToListAsync();
            //    productDenoList = _mapper.Map<List<StandardModel>>(list);
            //    // return list;
            //}
            //  var list = await _productDenominationRepository.GetPagedProductDenominationReponseAsync(model);

            result.UserDeclarationsModel = GetPage(UserStdList, model.Page, model.PageSize);
            result.TotalCount = UserStdList.Count();
            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public async Task<string> UserDeclarationDeleteById(long id)
    {
        // OzoneContext ozonedb = new OzoneContext();
        using (var transaction = _unitOfWork.BeginTransaction())
        {


            UserDeclaration dbresult = _dbContext.UserDeclaration.Where(u => u.Id == id).FirstOrDefault();


            if (dbresult != null)
            {
                // SecUser user = _secuserRepository.GetUserByUserName(input.UserName);

                try
                {


                    dbresult.IsDeleted = true;
                    await base.UpdateAsync(dbresult);
                    await _unitOfWork.SaveChangesAsync();





                    transaction.Commit();


                    return "Successfully Deleted!";

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return "Not Deleted!";
                }
            }
            else
            {
                return "Client not Exists!";
            }


        }
        // return "User Already Exists!";
    }
    public async Task<List<ContractTypeModel>> GetAllContractType()
    {
        var result = new List<ContractTypeModel>();

        var list = await Task.Run(() => _dbContext.ContractType.Where(x => x.IsActive == true).ToList());
        result = _mapper.Map<List<ContractTypeModel>>(list);
        return result;
    }
   
}
}

