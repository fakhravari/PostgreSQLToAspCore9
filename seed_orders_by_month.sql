-- پارامترها
DO $$
DECLARE
  target_year INT := 2024;
  orders_per_month INT := 8;
  c_id INT;
  o_id INT;
  d DATE;
  st TEXT;
  prod RECORD;
  i INT;
  j INT;
  items_count INT;
BEGIN
  FOR i IN 1..12 LOOP
    FOR j IN 1..orders_per_month LOOP
      -- مشتری تصادفی
      SELECT customer_id INTO c_id
      FROM customers ORDER BY random() LIMIT 1;

      -- تاریخ تصادفی در همان ماه
      d := make_date(target_year, i, 1) + (floor(random() * (date_part('day', (date_trunc('month', make_date(target_year, i, 1)) + INTERVAL '1 month - 1 day')::date)::int)))::int;

      -- وضعیت با وزن
      st := CASE
              WHEN random() < 0.55 THEN 'Paid'
              WHEN random() < 0.75 THEN 'Shipped'
              WHEN random() < 0.90 THEN 'Pending'
              ELSE 'Cancelled'
            END;

      -- سفارش
      INSERT INTO orders(customer_id, order_date, status) VALUES (c_id, d::timestamp, st)
      RETURNING order_id INTO o_id;

      -- 1..3 آیتم
      items_count := 1 + (floor(random()*3))::int;
      FOR j IN 1..items_count LOOP
        SELECT product_id, price INTO prod FROM products ORDER BY random() LIMIT 1;
        INSERT INTO order_items(order_id, product_id, qty, unit_price)
        VALUES (o_id, prod.product_id, 1 + (floor(random()*3))::int, prod.price);
      END LOOP;
    END LOOP;
  END LOOP;
END $$;

-- تست:
-- SELECT * FROM fn_monthly_sales(2025);
