using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly ISalesDocumentRepository _salesDocumentRepository;
        private readonly ApplicationDbContext _context;

        public DeliveryService(
            IDeliveryRepository deliveryRepository,
            ISalesDocumentRepository salesDocumentRepository,
            ApplicationDbContext context)
        {
            _deliveryRepository = deliveryRepository;
            _salesDocumentRepository = salesDocumentRepository;
            _context = context;
        }

        public async Task<Delivery?> GetDeliveryBySalesDocumentIdAsync(int salesDocumentId)
        {
            return await _deliveryRepository.GetDeliveryBySalesDocumentIdAsync(salesDocumentId);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByStatusAsync(string status)
        {
            return await _deliveryRepository.GetDeliveriesByStatusAsync(status);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _deliveryRepository.GetDeliveriesByDateRangeAsync(startDate, endDate);
        }

        public async Task<Delivery> CreateOrUpdateDeliveryAsync(int salesDocumentId, DateTime scheduledDate, string? handoverNote = null)
        {
            // Validate sales document exists
            var salesDocument = await _salesDocumentRepository.GetByIdAsync(salesDocumentId);
            if (salesDocument == null)
            {
                throw new KeyNotFoundException($"SalesDocument with ID {salesDocumentId} not found");
            }

            // Check if delivery already exists
            var existingDelivery = await _deliveryRepository.GetDeliveryBySalesDocumentIdAsync(salesDocumentId);

            if (existingDelivery != null)
            {
                // Update existing delivery
                existingDelivery.ScheduledDate = scheduledDate;
                existingDelivery.HandoverNote = handoverNote;
                existingDelivery.Status = "SCHEDULED";
                await _deliveryRepository.UpdateAsync(existingDelivery);
                return existingDelivery;
            }
            else
            {
                // Create new delivery
                var delivery = new Delivery
                {
                    SalesDocumentId = salesDocumentId,
                    ScheduledDate = scheduledDate,
                    Status = "SCHEDULED",
                    HandoverNote = handoverNote,
                    CreatedDate = DateTime.UtcNow
                };
                var createdDelivery = await _deliveryRepository.AddAsync(delivery);

                // Auto-update sales document status
                if (salesDocument.Type == "ORDER" && salesDocument.Status != "DELIVERED")
                {
                    salesDocument.Status = "DELIVERY_SCHEDULED";
                    salesDocument.UpdatedAt = DateTime.UtcNow;
                    await _salesDocumentRepository.UpdateAsync(salesDocument);
                }

                return createdDelivery;
            }
        }

        public async Task<Delivery> MarkDeliveryAsDeliveredAsync(int deliveryId, DateTime deliveredDate, string? handoverNote = null)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
            if (delivery == null)
            {
                throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
            }

            delivery.DeliveredDate = deliveredDate;
            delivery.Status = "DELIVERED";
            delivery.HandoverNote = handoverNote ?? delivery.HandoverNote;

            await _deliveryRepository.UpdateAsync(delivery);

            // Auto-update sales document status
            var salesDocument = await _salesDocumentRepository.GetByIdAsync(delivery.SalesDocumentId);
            if (salesDocument != null && salesDocument.Type == "ORDER")
            {
                salesDocument.Status = "DELIVERED";
                salesDocument.UpdatedAt = DateTime.UtcNow;
                await _salesDocumentRepository.UpdateAsync(salesDocument);
            }

            return delivery;
        }

        public async Task<Delivery> UpdateDeliveryStatusAsync(int deliveryId, string status)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
            if (delivery == null)
            {
                throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
            }

            // Validate status
            var validStatuses = new[] { "SCHEDULED", "IN_TRANSIT", "DELIVERED", "CANCELLED" };
            if (!validStatuses.Contains(status))
            {
                throw new ArgumentException($"Invalid delivery status: {status}", nameof(status));
            }

            delivery.Status = status;
            await _deliveryRepository.UpdateAsync(delivery);
            return delivery;
        }

        public async Task<Delivery> StartDeliveryAsync(int deliveryId)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
            if (delivery == null)
            {
                throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
            }

            if (delivery.Status != "SCHEDULED")
            {
                throw new InvalidOperationException($"Cannot start delivery. Current status: {delivery.Status}. Expected: SCHEDULED");
            }

            delivery.Status = "IN_TRANSIT";
            await _deliveryRepository.UpdateAsync(delivery);

            // Auto-update sales document status
            var salesDocument = await _salesDocumentRepository.GetByIdAsync(delivery.SalesDocumentId);
            if (salesDocument != null && salesDocument.Type == "ORDER")
            {
                salesDocument.Status = "IN_DELIVERY";
                salesDocument.UpdatedAt = DateTime.UtcNow;
                await _salesDocumentRepository.UpdateAsync(salesDocument);
            }

            return delivery;
        }

        public async Task<Delivery> CustomerConfirmReceiptAsync(int deliveryId)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
            if (delivery == null)
            {
                throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
            }

            if (delivery.Status != "IN_TRANSIT")
            {
                throw new InvalidOperationException($"Customer cannot confirm receipt. Current status: {delivery.Status}. Expected: IN_TRANSIT");
            }

            delivery.CustomerConfirmed = true;
            delivery.CustomerConfirmedDate = DateTime.UtcNow;
            await _deliveryRepository.UpdateAsync(delivery);

            return delivery;
        }

        public async Task<Delivery> CompleteDeliveryAsync(int deliveryId, DateTime deliveredDate, string? handoverNote = null)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
            if (delivery == null)
            {
                throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
            }

            if (delivery.Status != "IN_TRANSIT")
            {
                throw new InvalidOperationException($"Cannot complete delivery. Current status: {delivery.Status}. Expected: IN_TRANSIT");
            }

            if (!delivery.CustomerConfirmed)
            {
                throw new InvalidOperationException("Cannot complete delivery. Customer has not confirmed receipt yet.");
            }

            delivery.DeliveredDate = deliveredDate;
            delivery.Status = "DELIVERED";
            delivery.HandoverNote = handoverNote ?? delivery.HandoverNote;

            await _deliveryRepository.UpdateAsync(delivery);

            // Auto-update sales document status
            var salesDocument = await _salesDocumentRepository.GetByIdAsync(delivery.SalesDocumentId);
            if (salesDocument != null && salesDocument.Type == "ORDER")
            {
                salesDocument.Status = "DELIVERED";
                salesDocument.UpdatedAt = DateTime.UtcNow;
                await _salesDocumentRepository.UpdateAsync(salesDocument);
            }

            return delivery;
        }

        public async Task<bool> DeliveryExistsAsync(int id)
        {
            return await _deliveryRepository.ExistsAsync(id);
        }
    }
}

