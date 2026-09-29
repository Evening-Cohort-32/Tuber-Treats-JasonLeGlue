using System.ComponentModel.Design;
using TuberTreats.Models;
using TuberTreats.Models.DTOs;

List<TuberDriver> drivers = new List<TuberDriver>
{
    new TuberDriver { Id = 1, Name = "Mustard Mike", TuberDeliveries = new List<TuberOrder>() },
    new TuberDriver { Id = 2, Name = "Responsible Richard", TuberDeliveries = new List<TuberOrder>() },
    new TuberDriver { Id = 3, Name = "Sam Clam", TuberDeliveries = new List<TuberOrder>() }
};

List<Customer> customers = new List<Customer>
{
    new Customer { Id = 1, Name = "Ada Byrne",    Address = "412 Maple St",     TuberOrders = new List<TuberOrder>() },
    new Customer { Id = 2, Name = "Ken Lowell",   Address = "88 Warehouse Ave", TuberOrders = new List<TuberOrder>() },
    new Customer { Id = 3, Name = "Priya Nair",   Address = "1900 Belmont Blvd",TuberOrders = new List<TuberOrder>() },
    new Customer { Id = 4, Name = "Tomas Feld",   Address = "7 Oak Ridge Ct",   TuberOrders = new List<TuberOrder>() },
    new Customer { Id = 5, Name = "Joan Whitmer", Address = "233 Gallatin Pike",TuberOrders = new List<TuberOrder>() }
};

List<Topping> toppings = new List<Topping>
{
    new Topping { Id = 1, Name = "Cheddar Cheese" },
    new Topping { Id = 2, Name = "Sour Cream" },
    new Topping { Id = 3, Name = "Bacon Bits" },
    new Topping { Id = 4, Name = "Chives" },
    new Topping { Id = 5, Name = "Chili" }
};

List<TuberOrder> orders = new List<TuberOrder>
{
    // Delivered, has three toppings
    new TuberOrder
    {
        Id = 1,
        OrderPlacedOnDate = new DateTime(2026, 9, 8, 11, 45, 0),
        CustomerId = 1,
        TuberDriverId = 2,
        DeliveredOnDate = new DateTime(2026, 9, 8, 12, 20, 0),
        Toppings = toppings.Where(t => t.Id == 1 || t.Id == 2 || t.Id == 3).ToList()
    },
    // Assigned to a driver but not yet delivered, has two toppings
    new TuberOrder
    {
        Id = 2,
        OrderPlacedOnDate = new DateTime(2026, 9, 11, 18, 5, 0),
        CustomerId = 3,
        TuberDriverId = 1,
        DeliveredOnDate = null,
        Toppings = toppings.Where(t => t.Id == 4 || t.Id == 5).ToList()
    },
    // Unassigned, plain potato
    new TuberOrder
    {
        Id = 3,
        OrderPlacedOnDate = new DateTime(2026, 9, 12, 9, 30, 0),
        CustomerId = 5,
        TuberDriverId = null,
        DeliveredOnDate = null,
        Toppings = new List<Topping>()
    }
};

List<TuberTopping> tuberToppings = new List<TuberTopping>
{
    new TuberTopping
    {
        Id = 1,
        TuberOrderId = 1,
        ToppingId = 1
    },
    new TuberTopping
    {
        Id = 2,
        TuberOrderId = 1,
        ToppingId = 2
    },
    new TuberTopping
    {
        Id = 3,
        TuberOrderId = 1,
        ToppingId = 3
    },
    new TuberTopping
    {
        Id = 4,
        TuberOrderId = 2,
        ToppingId = 4
    },
    new TuberTopping
    {
        Id = 5,
        TuberOrderId = 2,
        ToppingId = 5
    },
};


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//add endpoints here


//tuberorders
app.MapGet("/api/tuberorders", () =>
{
    return orders.Select(o => new TuberOrderDTO
    {
        Id = o.Id,
        OrderPlacedOnDate = o.OrderPlacedOnDate,
        CustomerId = o.CustomerId,
        TuberDriverId = o.TuberDriverId,
        DeliveredOnDate = o.DeliveredOnDate,
        Toppings = o.Toppings.Select(t => new ToppingDTO
        {
            Id = t.Id,
            Name = t.Name
        }).ToList()
    });
});

app.MapGet("/api/tuberorders/{id}", (int id) =>
{
    TuberOrder tuberOrder = orders.FirstOrDefault(o => o.Id == id);

    if (tuberOrder == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new TuberOrderDTO
    {
        Id = tuberOrder.Id,
        OrderPlacedOnDate = tuberOrder.OrderPlacedOnDate,
        CustomerId = tuberOrder.CustomerId,
        TuberDriverId = tuberOrder.TuberDriverId,
        DeliveredOnDate = tuberOrder.DeliveredOnDate,
        Toppings = tuberOrder.Toppings.Select(t => new ToppingDTO
        {
            Id = t.Id,
            Name = t.Name
        }).ToList()
    });
});

//submit new order
//needs CustomerId at minimum, assigns Id, OrderPlacedOnDate, and Toppings if missing
app.MapPost("/api/tuberorders", (TuberOrder tuberOrder) =>
{
    tuberOrder.Id = orders.Max(o => o.Id) + 1;
    tuberOrder.OrderPlacedOnDate = DateTime.Now;
    
    if (tuberOrder.Toppings == null)
    {
        tuberOrder.Toppings = new List<Topping>();
    }

    orders.Add(tuberOrder);

    return Results.Created($"/api/tuberorders/{tuberOrder.Id}", new TuberOrderDTO
    {
        Id = tuberOrder.Id,
        OrderPlacedOnDate = tuberOrder.OrderPlacedOnDate,
        CustomerId = tuberOrder.CustomerId,
        Toppings = tuberOrder.Toppings.Select(t => new ToppingDTO
        {
            Id = t.Id,
            Name = t.Name
        }).ToList()
    });

});
//assign driver (put to /tuberoders/{id})
//Only needs TuberDriverId
app.MapPut("/api/tuberorders/{id}", (int id, TuberOrder tuberOrder) =>
{
    TuberOrder tuberOrderToUpdate = orders.FirstOrDefault(o => o.Id == id);
    if (tuberOrderToUpdate == null)
    {
        return Results.NotFound();
    }

    tuberOrderToUpdate.TuberDriverId = tuberOrder.TuberDriverId;

    return Results.NoContent();


});
//complete order (Post to /tuberorders/{id}/complete)
app.MapPost("/api/tuberorders/{id}/complete", (int id) =>
{
    TuberOrder tuberOrderToUpdate = orders.FirstOrDefault(o => o.Id == id);
    if (tuberOrderToUpdate == null)
    {
        return Results.BadRequest();
    }

    tuberOrderToUpdate.DeliveredOnDate = DateTime.Now;

    return Results.NoContent();
});


//toppings 
app.MapGet("/api/toppings", () =>
{
    return toppings.Select(t => new ToppingDTO
    {
        Id = t.Id,
        Name = t.Name
    });
});

app.MapGet("/api/toppings/{id}", (int id) =>
{
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);

    if (topping == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new ToppingDTO
    {
        Id = topping.Id,
        Name = topping.Name
    });
});

//tubertoppings
app.MapGet("/api/tubertoppings", () =>
{
    return tuberToppings.Select(t => new TuberToppingDTO
    {
        Id = t.Id,
        TuberOrderId = t.TuberOrderId,
        ToppingId = t.ToppingId

    });
});

app.MapPost("/api/tubertoppings/", (TuberTopping tuberTopping) =>
{
    tuberTopping.Id = tuberToppings.Max(tt => tt.Id) + 1;
    tuberToppings.Add(tuberTopping);

    Topping topping = new Topping
    {
        Id = tuberTopping.ToppingId,
        Name = toppings.FirstOrDefault(t => t.Id == tuberTopping.ToppingId).Name

    };

    TuberOrder tuberOrderToUpdate = orders.FirstOrDefault(to => to.Id == tuberTopping.TuberOrderId);

    if (tuberOrderToUpdate == null)
    {
        return Results.BadRequest();
    }
    tuberOrderToUpdate.Toppings.Add(topping);





    return Results.Created($"/api/tubertoppings/{tuberTopping.Id}", new TuberToppingDTO
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        ToppingId = tuberTopping.ToppingId
    });
});


app.MapDelete("/api/tubertoppings/{id}", (int id) =>
{
    TuberTopping tuberTopping = tuberToppings.FirstOrDefault(tt => tt.Id == id);
    if (tuberTopping == null)
    {
        return Results.BadRequest();
    }

    TuberOrder tuberOrderToUpdate = orders.FirstOrDefault(to => to.Id == tuberTopping.TuberOrderId);
    if (tuberOrderToUpdate == null)
    {
        return Results.BadRequest();
    }

    Topping topping = toppings.FirstOrDefault(t => t.Id == tuberTopping.ToppingId);

    tuberOrderToUpdate.Toppings.Remove(topping);

    tuberToppings.Remove(tuberTopping);

    return Results.NoContent();


});


//customers
app.MapGet("/api/customers", () =>
{
    return customers.Select(c => new CustomerDTO
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address,
        TuberOrders = orders.Where(o => o.CustomerId == c.Id).Select(o => new TuberOrderDTO
        {
            Id = o.Id,
            OrderPlacedOnDate = o.OrderPlacedOnDate,
            CustomerId = o.CustomerId,
            TuberDriverId = o.TuberDriverId,
            DeliveredOnDate = o.DeliveredOnDate,
            Toppings = o.Toppings.Select(t => new ToppingDTO
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        }).ToList()

    });
});

app.MapGet("/api/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        TuberOrders = orders.Where(o => o.CustomerId == customer.Id).Select(o => new TuberOrderDTO
        {
            Id = o.Id,
            OrderPlacedOnDate = o.OrderPlacedOnDate,
            CustomerId = o.CustomerId,
            TuberDriverId = o.TuberDriverId,
            DeliveredOnDate = o.DeliveredOnDate,
            Toppings = o.Toppings.Select(t => new ToppingDTO
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        }).ToList()
    });
});

app.MapPost("/api/customers", (Customer customer) =>
{
    customer.Id = customers.Max(c => c.Id) + 1;
    customers.Add(customer);

    return Results.Created($"/api/customers/{customer.Id}", new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        TuberOrders = orders.Where(o => o.CustomerId == customer.Id).Select(o => new TuberOrderDTO
        {
            Id = o.Id,
            OrderPlacedOnDate = o.OrderPlacedOnDate,
            CustomerId = o.CustomerId,
            TuberDriverId = o.TuberDriverId,
            DeliveredOnDate = o.DeliveredOnDate,
            Toppings = o.Toppings.Select(t => new ToppingDTO
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        }).ToList()

    });
});

app.MapDelete("/api/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    customers.Remove(customer);
    return Results.NoContent();
});

//tuber drivers
app.MapGet("/api/tuberdrivers", () =>
{
    return drivers.Select(d => new TuberDriverDTO
    {
        Id = d.Id,
        Name = d.Name,
        TuberDeliveries = d.TuberDeliveries.Select(to => new TuberOrderDTO
        {
            Id = to.Id,
            OrderPlacedOnDate = to.OrderPlacedOnDate,
            CustomerId = to.CustomerId,
            TuberDriverId = to.TuberDriverId,
            DeliveredOnDate = to.DeliveredOnDate,
            Toppings = to.Toppings.Select(t => new ToppingDTO
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        }).ToList()
    });
});

app.MapGet("/api/tuberdrivers/{id}", (int id) =>
{
    TuberDriver tuberDriver = drivers.FirstOrDefault(d => d.Id == id);

    if (tuberDriver == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new TuberDriverDTO
    {
        Id = tuberDriver.Id,
        Name = tuberDriver.Name,
        TuberDeliveries = tuberDriver.TuberDeliveries.Select(to => new TuberOrderDTO
        {
            Id = to.Id,
            OrderPlacedOnDate = to.OrderPlacedOnDate,
            CustomerId = to.CustomerId,
            TuberDriverId = to.TuberDriverId,
            DeliveredOnDate = to.DeliveredOnDate,
            Toppings = to.Toppings.Select(t => new ToppingDTO
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        }).ToList()

    });

});


app.Run();
//don't touch or move this!
public partial class Program { }