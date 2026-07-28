import StockAdjustmentForm from '../../StockAdjustmentForm';

export default async function EditStockAdjustmentPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <StockAdjustmentForm adjustmentId={id} />;
}
