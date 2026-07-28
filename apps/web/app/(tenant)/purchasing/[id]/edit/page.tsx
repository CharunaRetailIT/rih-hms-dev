import PurchaseOrderForm from '../../PurchaseOrderForm';

export default async function EditPurchaseOrderPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <PurchaseOrderForm poId={id} />;
}
