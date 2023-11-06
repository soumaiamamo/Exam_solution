using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
class Solution
{
    public static IQueryable<Dish> Q1(ExamContext db, string name, decimal minPrice, decimal maxPrice)
    {
        var query = from dish in db.FoodItems.OfType<Dish>()
                    where dish.Name.Contains(name) && dish.Price >= minPrice && dish.Price <= maxPrice
                    select dish; 
        return query;

    }
    
    public static IQueryable<DishAndCategory> Q2(ExamContext db, int customerId)
    {
        var query = from order in db.Orders
                    join foodItem in db.FoodItems on order.FoodItemID equals foodItem.ID
                    join category in db.Categories on foodItem.CategoryID equals category.ID
                    where order.CustomerID == customerId
                    select new 
                    {
                        Name = foodItem.Name,
                        Price = foodItem.Price,
                        Unit = foodItem.Unit,
                        CategoryName = category.Name
                    };

        return query.OrderBy(x => x.CategoryName).Select(x => new DishAndCategory(x.Name, x.Price, x.Unit, x.CategoryName));
    }

    //In DataFormats -> CustomerBill (BillItem)
    public static IQueryable<CustomerBill> Q3(ExamContext db, int number)
    {
        var query = from order in db.Orders
                  
                    join foodItem in db.FoodItems on order.FoodItemID equals foodItem.ID
                    group new { foodItem, order } by customer into customerGroup
                    let total = customerGroup.Sum(x => x.orderItem.Quantity * x.foodItem.Price)
                    orderby total descending
                    select new 
                    {
                        CustomerId = customerGroup.Key.ID,
                        BillItems = customerGroup.Select(x => new
                        {
                            Name = x.foodItem.Name,
                            Price = x.foodItem.Price,
                            Unit = x.foodItem.Unit,
                            Quantity = x.orderItem.Quantity
                        }).ToList(),
                        Total = total
                    };

        return query.Take(number);
    }

    public static IQueryable<Dish> Q4(ExamContext db, int tableNumber)
    {
        var query = from foodItem in db.FoodItems.OfType<Dish>()
                    join orderItem in db.Order on foodItem.ID equals order.FoodItemID into orderItems
                    from orderItem in orderItems.DefaultIfEmpty()
                    join order in db.Orders on orderItem.OrderID equals order.FoodItemID into orders
                    from order in orders.DefaultIfEmpty()
                    where order == null || order.TableNumber != tableNumber
                    orderby foodItem.Price
                    select foodItem;

        return query;
    }
  
    //In DataFormats -> record DishWithCategories

    public static void Q5(ExamContext db)
    {
        var query = from subCategory in db.Categories
                    join mainCategory in db.Categories on subCategory.CategoryID equals mainCategory.ID into mainCategories
                    from mainCategory in mainCategories.DefaultIfEmpty()
                    join foodItem in db.FoodItems.OfType<Dish>() on subCategory.ID equals foodItem.CategoryID into foodItems
                    from foodItem in foodItems.DefaultIfEmpty()
                    where subCategory.CategoryID != null
                    orderby mainCategory.Name, subCategory.Name, foodItem?.Name
                    select new 
                    {
                        MainCategoryName = mainCategory.Name,
                        SubCategoryName = subCategory.Name,
                        DishName = foodItem?.Name
                    };

    return query.ToList();
    }
    
    public static int Q6(ExamContext db, string firstCategory, string secondCategory) {
        var customer1 = new Customer { Name = "Customer 1" };
        var customer2 = new Customer { Name = "Customer 2" };
        db.Customers.Add(customer1);
        db.Customers.Add(customer2);

        db.SaveChanges();

    }
 
}