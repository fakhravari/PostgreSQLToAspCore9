-- اختیاری: ساخت دیتابیس و کانکت
-- CREATE DATABASE shopdb;
-- \c shopdb;

-- 1) جداول

CREATE TABLE IF NOT EXISTS categories (
  category_id  SERIAL PRIMARY KEY,
  name         VARCHAR(100) NOT NULL UNIQUE,
  slug         VARCHAR(100) UNIQUE
);

CREATE TABLE IF NOT EXISTS products (
  product_id   SERIAL PRIMARY KEY,
  name         VARCHAR(150) NOT NULL,
  price        NUMERIC(18,2) NOT NULL CHECK (price >= 0),
  category_id  INT NOT NULL REFERENCES categories(category_id)
);

CREATE INDEX IF NOT EXISTS ix_products_category_id ON products(category_id);

CREATE TABLE IF NOT EXISTS customers (
  customer_id  SERIAL PRIMARY KEY,
  full_name    VARCHAR(150) NOT NULL,
  email        VARCHAR(150) NOT NULL UNIQUE
);

-- وضعیت سفارش با CHECK (معادل enum)
CREATE TABLE IF NOT EXISTS orders (
  order_id     SERIAL PRIMARY KEY,
  customer_id  INT NOT NULL REFERENCES customers(customer_id),
  order_date   TIMESTAMP NOT NULL DEFAULT NOW(),
  status       VARCHAR(20) NOT NULL CHECK (status IN ('Pending','Paid','Shipped','Cancelled'))
);

CREATE INDEX IF NOT EXISTS ix_orders_customer_id ON orders(customer_id);
CREATE INDEX IF NOT EXISTS ix_orders_order_date  ON orders(order_date);
CREATE INDEX IF NOT EXISTS ix_orders_status      ON orders(status);

CREATE TABLE IF NOT EXISTS order_items (
  order_item_id  SERIAL PRIMARY KEY,
  order_id       INT NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
  product_id     INT NOT NULL REFERENCES products(product_id),
  qty            INT NOT NULL CHECK (qty > 0),
  unit_price     NUMERIC(18,2) NOT NULL CHECK (unit_price >= 0)
);

CREATE INDEX IF NOT EXISTS ix_order_items_order_id   ON order_items(order_id);
CREATE INDEX IF NOT EXISTS ix_order_items_product_id ON order_items(product_id);

-- 2) داده‌ی اولیه (Idempotent)

INSERT INTO categories (name, slug) VALUES
('Electronics','electronics'),
('Accessories','accessories'),
('Home Appliances','home-appliances'),
('Books','books'),
('Clothing','clothing')
ON CONFLICT (name) DO NOTHING;

INSERT INTO products (name, price, category_id)
SELECT * FROM (VALUES
('Laptop',1500, (SELECT category_id FROM categories WHERE name='Electronics')),
('Smartphone',900, (SELECT category_id FROM categories WHERE name='Electronics')),
('Tablet',600, (SELECT category_id FROM categories WHERE name='Electronics')),
('Mouse',25, (SELECT category_id FROM categories WHERE name='Accessories')),
('Keyboard',40, (SELECT category_id FROM categories WHERE name='Accessories')),
('Headphones',120, (SELECT category_id FROM categories WHERE name='Accessories')),
('Microwave',300, (SELECT category_id FROM categories WHERE name='Home Appliances')),
('Refrigerator',1200, (SELECT category_id FROM categories WHERE name='Home Appliances')),
('Washing Machine',800, (SELECT category_id FROM categories WHERE name='Home Appliances')),
('Novel: 1984',15, (SELECT category_id FROM categories WHERE name='Books')),
('Programming in C#',45, (SELECT category_id FROM categories WHERE name='Books')),
('T-shirt',20, (SELECT category_id FROM categories WHERE name='Clothing')),
('Jeans',50, (SELECT category_id FROM categories WHERE name='Clothing')),
('Jacket',100, (SELECT category_id FROM categories WHERE name='Clothing'))
) AS v(name, price, category_id)
ON CONFLICT DO NOTHING;

INSERT INTO customers (full_name, email) VALUES
('Ali Reza','ali@test.com'),
('Sara Ahmadi','sara@test.com'),
('Mohammad Hossein','mh@test.com'),
('Neda Karimi','neda@test.com'),
('Reza Pahlavan','reza@test.com')
ON CONFLICT (email) DO NOTHING;

-- یک سفارش نمونه
DO $$
DECLARE oid INT;
BEGIN
  INSERT INTO orders (customer_id, status, order_date)
  VALUES ((SELECT customer_id FROM customers WHERE email='ali@test.com'),'Paid', NOW())
  RETURNING order_id INTO oid;

  INSERT INTO order_items (order_id, product_id, qty, unit_price) VALUES
  (oid, (SELECT product_id FROM products WHERE name='Laptop'), 1, 1500),
  (oid, (SELECT product_id FROM products WHERE name='Mouse'),  2, 25),
  (oid, (SELECT product_id FROM products WHERE name='Keyboard'), 1, 40);
END $$;

-- 3) ویو معادل JOIN محصولات + دسته‌ها
CREATE OR REPLACE VIEW vw_products_with_category AS
SELECT
  p.product_id  AS idproduct,
  p.name        AS productname,
  c.name        AS categoriename,
  p.category_id AS idcategory
FROM products p
JOIN categories c ON p.category_id = c.category_id;

-- 4) تابع گزارش فروش ماهانه (Paid/Shipped)
CREATE OR REPLACE FUNCTION fn_monthly_sales(in_year INT)
RETURNS TABLE(year INT, month INT, total_sales NUMERIC, orders_count INT)
LANGUAGE sql AS $$
  WITH lines AS (
    SELECT o.order_id, o.order_date, oi.qty, oi.unit_price
    FROM orders o
    JOIN order_items oi ON o.order_id = oi.order_id
    WHERE EXTRACT(YEAR FROM o.order_date) = in_year
      AND o.status IN ('Paid','Shipped')
  )
  SELECT
    CAST(EXTRACT(YEAR FROM order_date) AS INT) AS year,
    CAST(EXTRACT(MONTH FROM order_date) AS INT) AS month,
    SUM(qty*unit_price) AS total_sales,
    COUNT(DISTINCT order_id) AS orders_count
  FROM lines
  GROUP BY year, month
  ORDER BY year, month;
$$;
