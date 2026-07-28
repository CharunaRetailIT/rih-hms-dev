import WastageForm from '../../WastageForm';

export default async function EditWastagePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <WastageForm wastageId={id} />;
}
