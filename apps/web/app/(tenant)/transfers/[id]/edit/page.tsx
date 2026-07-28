import TransferForm from '../../TransferForm';

export default async function EditTransferPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <TransferForm transferId={id} />;
}
