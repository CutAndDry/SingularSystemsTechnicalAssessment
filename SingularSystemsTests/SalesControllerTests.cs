using Microsoft.AspNetCore.Mvc;
using Moq;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;
using SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingularSystemsTests
{
    public class SalesControllerTests
    {
        //check if SalesController correctly handles the case when a sale with the given ID does not exist:
        [Fact]
        public async Task GetById_ReturnsNotFound_ForInvalidId()
        {
            // create mocked repositories
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Sale)null);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);

            var result = await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
