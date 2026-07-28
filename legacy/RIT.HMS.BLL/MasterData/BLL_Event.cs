using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Event
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Event()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Event(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<Event> GetEvents(Int32 compid)
        {
            try
            {
                IEnumerable<Event> evt = _unitofwork.EventRepository.Get(v=>v.CompanyID==compid).OrderBy(v => v.EventId);
                if (evt != null)
                {
                    return evt;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<Event> GetActiveEvents(Int32 compid)
        {
            try
            {
                IEnumerable<Event> events = _unitofwork.EventRepository.Get(e=> e.IsDelete==false && e.CompanyID==compid).OrderBy(c => c.EventName);
                return events ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Event GetEventById(long id)
        {
            try
            {
                Event _event = _unitofwork.EventRepository.Get(v => v.EventId == id).FirstOrDefault();
                if (_event != null)
                {
                    return _event;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<EventProduct> GetEventProductsByEventId(long id)
        {
            try
            {
                List<EventProduct> _eventproductlist = _unitofwork.EventProductRepository.Get(v => v.EventId == id).ToList();
                if (_eventproductlist != null)
                {
                    return _eventproductlist;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveEvent(Event evnt)
        {
            try
            {
                _unitofwork.EventRepository.Insert(evnt);
                if (evnt.EventProducts.Count != 0)
                {
                   _unitofwork.Save();
                }          
                foreach (EventProduct prd in evnt.EventProducts)
                {
                    prd.EventId = evnt.EventId;
                    prd.OrdSeq = evnt.EventProducts.IndexOf(prd) + 1;
                    prd.CreatedDate = evnt.CreatedDate;
                    prd.CreatedUser = evnt.CreatedUser;
                    prd.ModifiedDate = evnt.ModifiedDate;
                    prd.ModifiedUser = evnt.ModifiedUser;
                    prd.IsActive = true;
                    _unitofwork.EventProductRepository.Insert(prd);
                }
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateEvent(Event evt)
        {
            try
            {
                _unitofwork.EventRepository.Update(evt);
                if (evt.EventProducts.Count != 0)
                {
                    var exist = _unitofwork.EventProductRepository.Get(e => e.EventId.Equals(evt.EventId))
                                                                       .ToList();

                    foreach (EventProduct ep in evt.EventProducts)
                    {
                        if (!exist.Select(p => p.ProductId).Contains(ep.ProductId))
                        {
                            ep.EventId = evt.EventId;
                            ep.IsActive = true;
                            ep.ModifiedDate = evt.ModifiedDate;
                            ep.ModifiedUser = evt.ModifiedUser;
                            ep.CreatedDate = evt.CreatedDate;
                            ep.CreatedUser = evt.CreatedUser;
                            ep.OrdSeq = evt.EventProducts.IndexOf(ep) + 1;
                            _unitofwork.EventProductRepository.Insert(ep);
                        }
                    }

                    foreach (var ex in exist)
                    {
                        if (!evt.EventProducts.Select(p=>p.ProductId).Contains(ex.ProductId))
                        {

                            _unitofwork.EventProductRepository.Delete(ex);
                        }
                    }

                }

                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
