SET check_function_bodies = false;


DO language plpgsql $$BEGIN RAISE NOTICE 'Altering public.games_id_seq...';END$$;
ALTER SEQUENCE public.games_id_seq AS int4;
SET check_function_bodies = true;
