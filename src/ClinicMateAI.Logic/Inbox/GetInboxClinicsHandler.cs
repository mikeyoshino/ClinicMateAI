using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;

namespace ClinicMateAI.Logic.Inbox;

public sealed class GetInboxClinicsHandler(
    IClinicRepository clinicRepository) : IGetInboxClinicsHandler
{
    public async Task<IReadOnlyList<InboxClinicDto>> HandleAsync(
        GetInboxClinicsQuery query,
        CancellationToken cancellationToken = default)
    {
        var clinics = await clinicRepository.ListAsync(cancellationToken);
        return clinics
            .Select(x => new InboxClinicDto(x.Id, x.Name))
            .ToList();
    }
}
