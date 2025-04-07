-- Create shipments table if it doesn't exist
CREATE TABLE IF NOT EXISTS shipments (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    status VARCHAR(20) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Create index on order_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_shipments_order_id ON shipments(order_id);
