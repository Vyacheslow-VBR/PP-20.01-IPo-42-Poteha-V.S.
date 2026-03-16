--
-- PostgreSQL database dump
--

-- Dumped from database version 16.3
-- Dumped by pg_dump version 16.3

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

DROP DATABASE IF EXISTS poteha;
--
-- Name: poteha; Type: DATABASE; Schema: -; Owner: app2
--

CREATE DATABASE poteha WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'ru_RU.UTF-8';


ALTER DATABASE poteha OWNER TO app2;

\connect poteha

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: app; Type: SCHEMA; Schema: -; Owner: app2
--

CREATE SCHEMA app;


ALTER SCHEMA app OWNER TO app2;

--
-- Name: calculate_partner_discount_poteha(integer); Type: FUNCTION; Schema: app; Owner: app2
--

CREATE FUNCTION app.calculate_partner_discount_poteha(p_partner_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    total_quantity INTEGER;
    new_discount INTEGER;
BEGIN
    -- Считаем общее количество продаж для партнера
    SELECT COALESCE(SUM(quantity), 0) INTO total_quantity
    FROM sales_poteha
    WHERE partner_id = p_partner_id;
    
    -- Определяем скидку по шкале
    IF total_quantity < 10000 THEN
        new_discount := 0;
    ELSIF total_quantity < 50000 THEN
        new_discount := 5;
    ELSIF total_quantity < 300000 THEN
        new_discount := 10;
    ELSE
        new_discount := 15;
    END IF;
    
    RETURN new_discount;
END;
$$;


ALTER FUNCTION app.calculate_partner_discount_poteha(p_partner_id integer) OWNER TO app2;

--
-- Name: calculate_sale_total_poteha(); Type: FUNCTION; Schema: app; Owner: app2
--

CREATE FUNCTION app.calculate_sale_total_poteha() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    product_price DECIMAL(10, 2);
BEGIN
    SELECT price INTO product_price FROM products_poteha WHERE id = NEW.product_id;
    NEW.total_amount = NEW.quantity * product_price;
    RETURN NEW;
END;
$$;


ALTER FUNCTION app.calculate_sale_total_poteha() OWNER TO app2;

--
-- Name: trigger_update_partner_discount_poteha(); Type: FUNCTION; Schema: app; Owner: app2
--

CREATE FUNCTION app.trigger_update_partner_discount_poteha() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    affected_partner_id INTEGER;
BEGIN
    IF TG_OP = 'DELETE' THEN
        affected_partner_id := OLD.partner_id;
    ELSE
        affected_partner_id := NEW.partner_id;
    END IF;
    
    PERFORM update_partner_discount_poteha(affected_partner_id);
    RETURN NULL;
END;
$$;


ALTER FUNCTION app.trigger_update_partner_discount_poteha() OWNER TO app2;

--
-- Name: update_partner_discount_poteha(integer); Type: FUNCTION; Schema: app; Owner: app2
--

CREATE FUNCTION app.update_partner_discount_poteha(p_partner_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE partners_poteha
    SET discount = calculate_partner_discount_poteha(p_partner_id)
    WHERE id = p_partner_id;
END;
$$;


ALTER FUNCTION app.update_partner_discount_poteha(p_partner_id integer) OWNER TO app2;

--
-- Name: update_updated_at_column_poteha(); Type: FUNCTION; Schema: app; Owner: app2
--

CREATE FUNCTION app.update_updated_at_column_poteha() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;


ALTER FUNCTION app.update_updated_at_column_poteha() OWNER TO app2;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: partner_types_poteha; Type: TABLE; Schema: app; Owner: app2
--

CREATE TABLE app.partner_types_poteha (
    id integer NOT NULL,
    name character varying(100) NOT NULL
);


ALTER TABLE app.partner_types_poteha OWNER TO app2;

--
-- Name: partner_types_poteha_id_seq; Type: SEQUENCE; Schema: app; Owner: app2
--

CREATE SEQUENCE app.partner_types_poteha_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.partner_types_poteha_id_seq OWNER TO app2;

--
-- Name: partner_types_poteha_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app2
--

ALTER SEQUENCE app.partner_types_poteha_id_seq OWNED BY app.partner_types_poteha.id;


--
-- Name: partners_poteha; Type: TABLE; Schema: app; Owner: app2
--

CREATE TABLE app.partners_poteha (
    id integer NOT NULL,
    type_id integer NOT NULL,
    name character varying(200) NOT NULL,
    director_fullname character varying(200),
    phone character varying(20),
    email character varying(100),
    rating integer,
    discount integer DEFAULT 0,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    address text,
    CONSTRAINT partners_poteha_rating_check CHECK ((rating >= 0))
);


ALTER TABLE app.partners_poteha OWNER TO app2;

--
-- Name: partners_poteha_id_seq; Type: SEQUENCE; Schema: app; Owner: app2
--

CREATE SEQUENCE app.partners_poteha_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.partners_poteha_id_seq OWNER TO app2;

--
-- Name: partners_poteha_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app2
--

ALTER SEQUENCE app.partners_poteha_id_seq OWNED BY app.partners_poteha.id;


--
-- Name: products_poteha; Type: TABLE; Schema: app; Owner: app2
--

CREATE TABLE app.products_poteha (
    id integer NOT NULL,
    name character varying(200) NOT NULL,
    article character varying(50),
    price numeric(10,2),
    CONSTRAINT products_poteha_price_check CHECK ((price > (0)::numeric))
);


ALTER TABLE app.products_poteha OWNER TO app2;

--
-- Name: products_poteha_id_seq; Type: SEQUENCE; Schema: app; Owner: app2
--

CREATE SEQUENCE app.products_poteha_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.products_poteha_id_seq OWNER TO app2;

--
-- Name: products_poteha_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app2
--

ALTER SEQUENCE app.products_poteha_id_seq OWNED BY app.products_poteha.id;


--
-- Name: sales_poteha; Type: TABLE; Schema: app; Owner: app2
--

CREATE TABLE app.sales_poteha (
    id integer NOT NULL,
    partner_id integer NOT NULL,
    product_id integer NOT NULL,
    quantity integer NOT NULL,
    sale_date date DEFAULT CURRENT_DATE NOT NULL,
    total_amount numeric(10,2),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT sales_poteha_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE app.sales_poteha OWNER TO app2;

--
-- Name: sales_poteha_id_seq; Type: SEQUENCE; Schema: app; Owner: app2
--

CREATE SEQUENCE app.sales_poteha_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.sales_poteha_id_seq OWNER TO app2;

--
-- Name: sales_poteha_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app2
--

ALTER SEQUENCE app.sales_poteha_id_seq OWNED BY app.sales_poteha.id;


--
-- Name: partner_types_poteha id; Type: DEFAULT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.partner_types_poteha ALTER COLUMN id SET DEFAULT nextval('app.partner_types_poteha_id_seq'::regclass);


--
-- Name: partners_poteha id; Type: DEFAULT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.partners_poteha ALTER COLUMN id SET DEFAULT nextval('app.partners_poteha_id_seq'::regclass);


--
-- Name: products_poteha id; Type: DEFAULT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.products_poteha ALTER COLUMN id SET DEFAULT nextval('app.products_poteha_id_seq'::regclass);


--
-- Name: sales_poteha id; Type: DEFAULT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.sales_poteha ALTER COLUMN id SET DEFAULT nextval('app.sales_poteha_id_seq'::regclass);


--
-- Data for Name: partner_types_poteha; Type: TABLE DATA; Schema: app; Owner: app2
--

COPY app.partner_types_poteha (id, name) FROM stdin;
1	ООО
2	ЗАО
3	ИП
4	АО
5	ПАО
\.


--
-- Data for Name: partners_poteha; Type: TABLE DATA; Schema: app; Owner: app2
--

COPY app.partners_poteha (id, type_id, name, director_fullname, phone, email, rating, discount, created_at, updated_at, address) FROM stdin;
2	2	ТехноПром	Петров Петр Петрович	+7(999)765-43-21	info@techno.ru	4	5	2026-03-15 00:24:16.138185	2026-03-15 01:27:21.60455	\N
1	1	Ромашка	Иванов Иван Иванович	+7(999)123-45-67	info@romashka.ru	5	5	2026-03-15 00:24:16.138185	2026-03-15 01:27:53.55274	\N
5	4	34534	345	345	345	435	0	2026-03-15 11:36:37.032717	2026-03-15 16:41:00.586543	указан
6	3	23523	35353	353535	353535	35	0	2026-03-15 11:41:14.36024	2026-03-15 11:41:14.36024	353535
\.


--
-- Data for Name: products_poteha; Type: TABLE DATA; Schema: app; Owner: app2
--

COPY app.products_poteha (id, name, article, price) FROM stdin;
1	Продукт 1	ART001	1000.00
2	Продукт 2	ART002	2500.00
3	Продукт 3	ART003	500.00
4	Продукт 4	ART004	3000.00
5	Продукт 5	ART005	1500.00
\.


--
-- Data for Name: sales_poteha; Type: TABLE DATA; Schema: app; Owner: app2
--

COPY app.sales_poteha (id, partner_id, product_id, quantity, sale_date, total_amount, created_at) FROM stdin;
2	1	2	3000	2024-02-20	7500000.00	2026-03-15 00:24:16.156963
3	1	3	8000	2024-03-10	4000000.00	2026-03-15 00:24:16.156963
5	2	5	15000	2024-02-28	22500000.00	2026-03-15 00:24:16.156963
8	2	4	1000	2026-03-01	3000000.00	2026-03-14 20:07:25.095496
4	2	4	2000	2024-01-25	6000000.00	2026-03-15 00:24:16.156963
\.


--
-- Name: partner_types_poteha_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app2
--

SELECT pg_catalog.setval('app.partner_types_poteha_id_seq', 5, true);


--
-- Name: partners_poteha_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app2
--

SELECT pg_catalog.setval('app.partners_poteha_id_seq', 6, true);


--
-- Name: products_poteha_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app2
--

SELECT pg_catalog.setval('app.products_poteha_id_seq', 5, true);


--
-- Name: sales_poteha_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app2
--

SELECT pg_catalog.setval('app.sales_poteha_id_seq', 8, true);


--
-- Name: partner_types_poteha partner_types_poteha_name_key; Type: CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.partner_types_poteha
    ADD CONSTRAINT partner_types_poteha_name_key UNIQUE (name);


--
-- Name: partner_types_poteha partner_types_poteha_pkey; Type: CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.partner_types_poteha
    ADD CONSTRAINT partner_types_poteha_pkey PRIMARY KEY (id);


--
-- Name: partners_poteha partners_poteha_pkey; Type: CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.partners_poteha
    ADD CONSTRAINT partners_poteha_pkey PRIMARY KEY (id);


--
-- Name: products_poteha products_poteha_article_key; Type: CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.products_poteha
    ADD CONSTRAINT products_poteha_article_key UNIQUE (article);


--
-- Name: products_poteha products_poteha_pkey; Type: CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.products_poteha
    ADD CONSTRAINT products_poteha_pkey PRIMARY KEY (id);


--
-- Name: sales_poteha sales_poteha_pkey; Type: CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.sales_poteha
    ADD CONSTRAINT sales_poteha_pkey PRIMARY KEY (id);


--
-- Name: idx_partners_type_poteha; Type: INDEX; Schema: app; Owner: app2
--

CREATE INDEX idx_partners_type_poteha ON app.partners_poteha USING btree (type_id);


--
-- Name: idx_sales_date_poteha; Type: INDEX; Schema: app; Owner: app2
--

CREATE INDEX idx_sales_date_poteha ON app.sales_poteha USING btree (sale_date);


--
-- Name: idx_sales_partner_poteha; Type: INDEX; Schema: app; Owner: app2
--

CREATE INDEX idx_sales_partner_poteha ON app.sales_poteha USING btree (partner_id);


--
-- Name: idx_sales_product_poteha; Type: INDEX; Schema: app; Owner: app2
--

CREATE INDEX idx_sales_product_poteha ON app.sales_poteha USING btree (product_id);


--
-- Name: sales_poteha calculate_sale_total_before_insert_poteha; Type: TRIGGER; Schema: app; Owner: app2
--

CREATE TRIGGER calculate_sale_total_before_insert_poteha BEFORE INSERT ON app.sales_poteha FOR EACH ROW EXECUTE FUNCTION app.calculate_sale_total_poteha();


--
-- Name: sales_poteha trigger_update_partner_discount_poteha; Type: TRIGGER; Schema: app; Owner: app2
--

CREATE TRIGGER trigger_update_partner_discount_poteha AFTER INSERT OR DELETE OR UPDATE ON app.sales_poteha FOR EACH ROW EXECUTE FUNCTION app.trigger_update_partner_discount_poteha();


--
-- Name: partners_poteha update_partners_updated_at_poteha; Type: TRIGGER; Schema: app; Owner: app2
--

CREATE TRIGGER update_partners_updated_at_poteha BEFORE UPDATE ON app.partners_poteha FOR EACH ROW EXECUTE FUNCTION app.update_updated_at_column_poteha();


--
-- Name: partners_poteha partners_poteha_type_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.partners_poteha
    ADD CONSTRAINT partners_poteha_type_id_fkey FOREIGN KEY (type_id) REFERENCES app.partner_types_poteha(id) ON DELETE RESTRICT;


--
-- Name: sales_poteha sales_poteha_partner_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.sales_poteha
    ADD CONSTRAINT sales_poteha_partner_id_fkey FOREIGN KEY (partner_id) REFERENCES app.partners_poteha(id) ON DELETE CASCADE;


--
-- Name: sales_poteha sales_poteha_product_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app2
--

ALTER TABLE ONLY app.sales_poteha
    ADD CONSTRAINT sales_poteha_product_id_fkey FOREIGN KEY (product_id) REFERENCES app.products_poteha(id) ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

