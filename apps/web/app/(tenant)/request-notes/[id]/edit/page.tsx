import RequestNoteForm from '../../RequestNoteForm';

export default async function EditRequestNotePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <RequestNoteForm requestId={id} />;
}
