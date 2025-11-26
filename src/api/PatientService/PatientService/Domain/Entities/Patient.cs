using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using PatientApp.SharedKernel.Domain;
using PatientApp.SharedKernel.Results;
using PatientService.API.Domain.Events;
using PatientService.API.Domain.ValueObjects;

namespace PatientService.API.Domain.Entities;

internal sealed class Patient: Entity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required DateOnly DateOfBirth { get; set; }

    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    public MedicalAidDetails? MedicalAidDetails { get; } = new();
    
    public Result<PrivateHealthFundAccount> AddPrivateHealthFuncAccount(string ProviderName, string AccountNumber)
    {
        var existingAccount = MedicalAidDetails?.PrivateHealthFundAccounts
            .FirstOrDefault(a => a.ProviderName.Equals(ProviderName, StringComparison.OrdinalIgnoreCase)
                              && a.AccountNumber.Equals(AccountNumber, StringComparison.OrdinalIgnoreCase));

        if (existingAccount is not null)
        {
            return new Error("The private health fund account already exists.");
        }

        var newAccount = new PrivateHealthFundAccount(ProviderName, AccountNumber);
        MedicalAidDetails?.PrivateHealthFundAccounts.Add(newAccount);
        return newAccount;

    }

    public Result<bool> AddMedicareDetails(string medicareCardNumber, int medicareCardReferenceNumber)
    {
        if (string.IsNullOrWhiteSpace(medicareCardNumber))
        {
            return new Error("Medicare card number cannot be empty.");
        }
        if (medicareCardReferenceNumber <= 0)
        {
            return new Error("Medicare card reference number must be a positive integer.");
        }
        MedicalAidDetails!.MedicareCardNumber = medicareCardNumber;
        MedicalAidDetails.MedicareCardReferenceNumber = medicareCardReferenceNumber;
        return true;
    }
    
    public static Patient Create(string firstName, string lastName, DateOnly dateOfBirth, string email,
        string medicareNumber, int medicareReference)
    {
        var patient = new Patient()
        {
            FirstName =  firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Email = email
        };

        patient.AddMedicareDetails(medicareNumber, medicareReference);
        
        patient.AddDomainEvent(new NewPatientRegisteredEvent(PatientId:patient.Id,Email:patient.Email));
        
        return patient;
    }
}
