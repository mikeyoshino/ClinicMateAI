using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class ConversationRepository(AppDbContext dbContext) : IConversationRepository
{
    public Task<Conversation?> GetByIdAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        return dbContext.Conversations
            .FirstOrDefaultAsync(
                x => x.ClinicId == clinicId && x.Id == conversationId,
                cancellationToken);
    }

    public Task<Conversation?> GetByIdAsync(Guid clinicId, Guid branchId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        return dbContext.Conversations
            .FirstOrDefaultAsync(
                x => x.ClinicId == clinicId && x.BranchId == branchId && x.Id == conversationId,
                cancellationToken);
    }

    public Task<Conversation?> GetByExternalIdAsync(
        Guid clinicId,
        string channel,
        string externalConversationId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Conversations
            .FirstOrDefaultAsync(
                x => x.ClinicId == clinicId
                    && x.Channel == channel
                    && x.ExternalConversationId == externalConversationId,
                cancellationToken);
    }

    public Task<Conversation?> GetByExternalIdAsync(
        Guid clinicId,
        Guid branchId,
        string channel,
        string externalConversationId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Conversations
            .FirstOrDefaultAsync(
                x => x.ClinicId == clinicId
                    && x.BranchId == branchId
                    && x.Channel == channel
                    && x.ExternalConversationId == externalConversationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> ListRecentAsync(
        Guid clinicId,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .Where(x => x.ClinicId == clinicId)
            .OrderByDescending(x => x.LastMessageAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> ListRecentAsync(
        Guid clinicId,
        Guid? branchId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Conversations.Where(x => x.ClinicId == clinicId);

        if (branchId is not null)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        return await query
            .OrderByDescending(x => x.LastMessageAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        return dbContext.Conversations.AddAsync(conversation, cancellationToken).AsTask();
    }

    public void Update(Conversation conversation)
    {
        dbContext.Conversations.Update(conversation);
    }
}
